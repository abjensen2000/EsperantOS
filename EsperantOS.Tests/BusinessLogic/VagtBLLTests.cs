using EsperantOS.BusinessLogic;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DTO.Model;
using EsperantOS.Models;
using Moq;

namespace EsperantOS.Tests.BusinessLogic;

public class VagtBLLTests
{
    private readonly Mock<IUnitOfWork>          _mockUoW;
    private readonly Mock<IVagtRepository>      _mockVagtRepo;
    private readonly Mock<IMedarbejderRepository> _mockMedRepo;
    private readonly VagtBLL _bll;

    public VagtBLLTests()
    {
        _mockUoW       = new Mock<IUnitOfWork>();
        _mockVagtRepo  = new Mock<IVagtRepository>();
        _mockMedRepo   = new Mock<IMedarbejderRepository>();
        _mockUoW.Setup(u => u.VagtRepository).Returns(_mockVagtRepo.Object);
        _mockUoW.Setup(u => u.MedarbejderRepository).Returns(_mockMedRepo.Object);
        _bll = new VagtBLL(_mockUoW.Object);
    }

    private static DateTime GetNextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    private static Vagt MakeVagt(int id, DateTime dato, bool ædru = false, bool frigivet = false) =>
        new Vagt { Id = id, Dato = dato, Ædru = ædru, Frigivet = frigivet, Medarbejdere = new List<Medarbejder>() };

    // ── GetAllVagterAsync ─────────────────────────────────────

    [Fact]
    public async Task GetAllVagterAsync_ReturnsDtoListForAllEntities()
    {
        var entities = new List<Vagt>
        {
            MakeVagt(1, DateTime.Now),
            MakeVagt(2, DateTime.Now.AddHours(4))
        };
        _mockVagtRepo.Setup(r => r.GetAllVagterWithMedarbejdereAsync()).ReturnsAsync(entities);

        var result = await _bll.GetAllVagterAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
    }

    // ── GetFridayVagterAsync ──────────────────────────────────

    [Fact]
    public async Task GetFridayVagterAsync_ReturnsDtoListForFridayVagter()
    {
        var friday = GetNextFriday().AddHours(20);
        var entities = new List<Vagt> { MakeVagt(1, friday) };
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(entities);

        var result = await _bll.GetFridayVagterAsync();

        Assert.Single(result);
        Assert.Equal(friday, result[0].Dato);
    }

    // ── GetVagtByIdAsync ──────────────────────────────────────

    [Fact]
    public async Task GetVagtByIdAsync_Found_ReturnsDto()
    {
        var entity = MakeVagt(5, DateTime.Now);
        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(5)).ReturnsAsync(entity);

        var result = await _bll.GetVagtByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public async Task GetVagtByIdAsync_NotFound_ReturnsNull()
    {
        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(99)).ReturnsAsync((Vagt?)null);

        var result = await _bll.GetVagtByIdAsync(99);

        Assert.Null(result);
    }

    // ── GetVagterByMedarbejderAsync ───────────────────────────

    [Fact]
    public async Task GetVagterByMedarbejderAsync_ReturnsMappedList()
    {
        var entities = new List<Vagt> { MakeVagt(1, DateTime.Now) };
        _mockVagtRepo.Setup(r => r.GetVagterByMedarbejderAsync(3)).ReturnsAsync(entities);

        var result = await _bll.GetVagterByMedarbejderAsync(3);

        Assert.Single(result);
    }

    // ── GetVagterByMedarbejderNameAsync ───────────────────────

    [Fact]
    public async Task GetVagterByMedarbejderNameAsync_EmployeeFound_ReturnsMappedList()
    {
        var medarbejder = new Medarbejder { Id = 1, Name = "Simon", Vagter = new List<Vagt>() };
        var vagter      = new List<Vagt> { MakeVagt(10, DateTime.Now) };

        _mockMedRepo.Setup(r => r.GetMedarbejderByNameAsync("Simon")).ReturnsAsync(medarbejder);
        _mockVagtRepo.Setup(r => r.GetVagterByMedarbejderAsync(1)).ReturnsAsync(vagter);

        var result = await _bll.GetVagterByMedarbejderNameAsync("Simon");

        Assert.Single(result);
    }

    [Fact]
    public async Task GetVagterByMedarbejderNameAsync_EmployeeNotFound_ReturnsEmptyList()
    {
        _mockMedRepo.Setup(r => r.GetMedarbejderByNameAsync("Ukendt")).ReturnsAsync((Medarbejder?)null);

        var result = await _bll.GetVagterByMedarbejderNameAsync("Ukendt");

        Assert.Empty(result);
    }

    // ── EnsureAedruVagterAsync ────────────────────────────────

    [Fact]
    public async Task EnsureAedruVagterAsync_AllFridaysAlreadyHaveAedruVagt_NoNewVagtAdded()
    {
        var friday = GetNextFriday().AddHours(20);
        var vagter = new List<Vagt>
        {
            MakeVagt(1, friday, ædru: false),
            MakeVagt(2, friday, ædru: true)  // ædru vagt eksisterer allerede
        };
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(vagter);

        await _bll.EnsureAedruVagterAsync();

        _mockVagtRepo.Verify(r => r.AddAsync(It.IsAny<Vagt>()), Times.Never);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EnsureAedruVagterAsync_FridayMissingAedruVagt_CreatesOne()
    {
        var friday = GetNextFriday().AddHours(20);
        var vagter = new List<Vagt>
        {
            MakeVagt(1, friday, ædru: false),
            MakeVagt(2, friday, ædru: false)  // ingen ædru vagt
        };
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(vagter);

        await _bll.EnsureAedruVagterAsync();

        _mockVagtRepo.Verify(r => r.AddAsync(It.Is<Vagt>(v => v.Ædru && v.Frigivet)), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureAedruVagterAsync_NoFridayVagter_DoesNothing()
    {
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(new List<Vagt>());

        await _bll.EnsureAedruVagterAsync();

        _mockVagtRepo.Verify(r => r.AddAsync(It.IsAny<Vagt>()), Times.Never);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    // ── CreateVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task CreateVagtAsync_CallsAddAsyncAndSaveChanges()
    {
        var dto = new VagtDTO { Dato = DateTime.Now, Ædru = false, Frigivet = true, Medarbejdere = new List<MedarbejderDTO>() };

        await _bll.CreateVagtAsync(dto);

        _mockVagtRepo.Verify(r => r.AddAsync(It.IsAny<Vagt>()), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── UpdateVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task UpdateVagtAsync_VagtNotFound_DoesNotSave()
    {
        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(99)).ReturnsAsync((Vagt?)null);
        var dto = new VagtDTO { Id = 99, Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() };

        await _bll.UpdateVagtAsync(dto);

        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateVagtAsync_VagtFound_UpdatesFieldsAndSaves()
    {
        var existing    = MakeVagt(1, DateTime.Now);
        var medarbejder = new Medarbejder { Id = 5, Name = "Simon", Vagter = new List<Vagt>() };
        var dto = new VagtDTO
        {
            Id        = 1,
            Dato      = new DateTime(2025, 1, 3, 20, 0, 0),
            Ædru      = true,
            Frigivet  = false,
            Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 5, Name = "Simon" } }
        };

        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(existing);
        _mockMedRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(medarbejder);

        await _bll.UpdateVagtAsync(dto);

        Assert.Equal(dto.Dato, existing.Dato);
        Assert.True(existing.Ædru);
        Assert.Contains(medarbejder, existing.Medarbejdere);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateVagtAsync_MedarbejderNotFound_SkipsIt()
    {
        var existing = MakeVagt(1, DateTime.Now);
        var dto = new VagtDTO
        {
            Id           = 1,
            Dato         = DateTime.Now,
            Medarbejdere = new List<MedarbejderDTO> { new MedarbejderDTO { Id = 99 } }
        };

        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(existing);
        _mockMedRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Medarbejder?)null);

        await _bll.UpdateVagtAsync(dto);

        Assert.Empty(existing.Medarbejdere);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── DeleteVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task DeleteVagtAsync_CallsDeleteAsyncAndSaveChanges()
    {
        await _bll.DeleteVagtAsync(3);

        _mockVagtRepo.Verify(r => r.DeleteAsync(3), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── VagtExistsAsync ───────────────────────────────────────

    [Fact]
    public async Task VagtExistsAsync_VagtExists_ReturnsTrue()
    {
        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(MakeVagt(1, DateTime.Now));

        var result = await _bll.VagtExistsAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task VagtExistsAsync_VagtDoesNotExist_ReturnsFalse()
    {
        _mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(99)).ReturnsAsync((Vagt?)null);

        var result = await _bll.VagtExistsAsync(99);

        Assert.False(result);
    }
}
