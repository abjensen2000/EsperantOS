using EsperantOS.BusinessLogic;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DTO.Model;
using EsperantOS.Models;
using Moq;

namespace EsperantOS.Tests.BusinessLogic;

public class VagtBLLTests
{
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly Mock<IVagtRepository> _mockVagtRepo;
    private readonly Mock<IMedarbejderRepository> _mockMedRepo;
    private readonly VagtBLL _bll;

    public VagtBLLTests()
    {
        _mockUoW = new Mock<IUnitOfWork>();
        _mockVagtRepo = new Mock<IVagtRepository>();
        _mockMedRepo = new Mock<IMedarbejderRepository>();
        _mockUoW.Setup(u => u.VagtRepository).Returns(_mockVagtRepo.Object);
        _mockUoW.Setup(u => u.MedarbejderRepository).Returns(_mockMedRepo.Object);
        _bll = new VagtBLL(_mockUoW.Object);
    }

    private static DateTime NextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    private static Vagt MakeVagt(int id, DateTime dato, bool ædru = false, bool frigivet = false) =>
        new Vagt { Id = id, Dato = dato, Ædru = ædru, Frigivet = frigivet, Medarbejdere = new List<Medarbejder>() };

    // ── EnsureAedruVagterAsync ────────────────────────────────

    [Fact]
    public async Task EnsureAedruVagter_Test1()
    {
        var friday = NextFriday().AddHours(20);
        var vagter = new List<Vagt> { MakeVagt(1, friday), MakeVagt(2, friday) };
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(vagter);

        await _bll.EnsureAedruVagterAsync();

        _mockVagtRepo.Verify(r => r.AddAsync(It.Is<Vagt>(v => v.Ædru && v.Frigivet)), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EnsureAedruVagter_Test2()
    {
        var friday = NextFriday().AddHours(20);
        var vagter = new List<Vagt> { MakeVagt(1, friday), MakeVagt(2, friday, ædru: true) };
        _mockVagtRepo.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(vagter);

        await _bll.EnsureAedruVagterAsync();

        _mockVagtRepo.Verify(r => r.AddAsync(It.IsAny<Vagt>()), Times.Never);
    }

    // ── CreateVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task CreateVagt_Test()
    {
        var dto = new VagtDTO { Dato = DateTime.Now, Medarbejdere = new List<MedarbejderDTO>() };

        await _bll.CreateVagtAsync(dto);

        _mockVagtRepo.Verify(r => r.AddAsync(It.IsAny<Vagt>()), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── UpdateVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task UpdateVagt_Test()
    {
        var existing = MakeVagt(1, DateTime.Now);
        var medarbejder = new Medarbejder { Id = 5, Name = "Simon", Vagter = new List<Vagt>() };
        var dto = new VagtDTO
        {
            Id = 1,
            Dato = new DateTime(2025, 1, 3, 20, 0, 0),
            Ædru = true,
            Frigivet = false,
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

    // ── DeleteVagtAsync ───────────────────────────────────────

    [Fact]
    public async Task DeleteVagt_Test()
    {
        await _bll.DeleteVagtAsync(3);

        _mockVagtRepo.Verify(r => r.DeleteAsync(3), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}