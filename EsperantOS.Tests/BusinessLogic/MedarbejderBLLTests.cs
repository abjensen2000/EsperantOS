using EsperantOS.BusinessLogic;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DTO.Model;
using EsperantOS.Models;
using Moq;

namespace EsperantOS.Tests.BusinessLogic;

public class MedarbejderBLLTests
{
    private readonly Mock<IUnitOfWork> _mockUoW;
    private readonly Mock<IMedarbejderRepository> _mockRepo;
    private readonly MedarbejderBLL _bll;

    public MedarbejderBLLTests()
    {
        _mockUoW  = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IMedarbejderRepository>();
        _mockUoW.Setup(u => u.MedarbejderRepository).Returns(_mockRepo.Object);
        _bll = new MedarbejderBLL(_mockUoW.Object);
    }

    // ── GetAllMedarbejdereAsync ───────────────────────────────

    [Fact]
    public async Task GetAllMedarbejdereAsync_ReturnsDtoListForAllEntities()
    {
        var entities = new List<Medarbejder>
        {
            new Medarbejder { Id = 1, Name = "Simon", Bestyrelsesmedlem = false, Vagter = new List<Vagt>() },
            new Medarbejder { Id = 2, Name = "Mads",  Bestyrelsesmedlem = true,  Vagter = new List<Vagt>() }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        var result = await _bll.GetAllMedarbejdereAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Simon", result[0].Name);
        Assert.Equal("Mads",  result[1].Name);
    }

    [Fact]
    public async Task GetAllMedarbejdereAsync_EmptyRepository_ReturnsEmptyList()
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Medarbejder>());

        var result = await _bll.GetAllMedarbejdereAsync();

        Assert.Empty(result);
    }

    // ── GetMedarbejderByIdAsync ───────────────────────────────

    [Fact]
    public async Task GetMedarbejderByIdAsync_Found_ReturnsDto()
    {
        var entity = new Medarbejder { Id = 1, Name = "Simon", Bestyrelsesmedlem = false, Vagter = new List<Vagt>() };
        _mockRepo.Setup(r => r.GetMedarbejderWithVagterAsync(1)).ReturnsAsync(entity);

        var result = await _bll.GetMedarbejderByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Simon", result.Name);
    }

    [Fact]
    public async Task GetMedarbejderByIdAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetMedarbejderWithVagterAsync(99)).ReturnsAsync((Medarbejder?)null);

        var result = await _bll.GetMedarbejderByIdAsync(99);

        Assert.Null(result);
    }

    // ── GetMedarbejderByNameAsync ─────────────────────────────

    [Fact]
    public async Task GetMedarbejderByNameAsync_Found_ReturnsDto()
    {
        var entity = new Medarbejder { Id = 1, Name = "Simon", Vagter = new List<Vagt>() };
        _mockRepo.Setup(r => r.GetMedarbejderByNameAsync("Simon")).ReturnsAsync(entity);

        var result = await _bll.GetMedarbejderByNameAsync("Simon");

        Assert.NotNull(result);
        Assert.Equal("Simon", result!.Name);
    }

    [Fact]
    public async Task GetMedarbejderByNameAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetMedarbejderByNameAsync("Ukendt")).ReturnsAsync((Medarbejder?)null);

        var result = await _bll.GetMedarbejderByNameAsync("Ukendt");

        Assert.Null(result);
    }

    // ── GetBestyrelsesmedlemmerAsync ──────────────────────────

    [Fact]
    public async Task GetBestyrelsesmedlemmerAsync_ReturnsBoardMembersOnly()
    {
        var entities = new List<Medarbejder>
        {
            new Medarbejder { Id = 2, Name = "Mads", Bestyrelsesmedlem = true, Vagter = new List<Vagt>() }
        };
        _mockRepo.Setup(r => r.GetBestyrelsesmedlemmerAsync()).ReturnsAsync(entities);

        var result = await _bll.GetBestyrelsesmedlemmerAsync();

        Assert.Single(result);
        Assert.True(result[0].Bestyrelsesmedlem);
    }

    // ── CreateMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task CreateMedarbejderAsync_CallsAddAsyncAndSaveChanges()
    {
        var dto = new MedarbejderDTO { Name = "Emma", Bestyrelsesmedlem = false, Vagter = new List<VagtDTO>() };

        await _bll.CreateMedarbejderAsync(dto);

        _mockRepo.Verify(r => r.AddAsync(It.Is<Medarbejder>(m => m.Name == "Emma")), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── UpdateMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task UpdateMedarbejderAsync_CallsUpdateAsyncAndSaveChanges()
    {
        var dto = new MedarbejderDTO { Id = 1, Name = "Simon", Bestyrelsesmedlem = true, Vagter = new List<VagtDTO>() };

        await _bll.UpdateMedarbejderAsync(dto);

        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Medarbejder>(m => m.Id == 1 && m.Name == "Simon")), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── DeleteMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task DeleteMedarbejderAsync_CallsDeleteAsyncAndSaveChanges()
    {
        await _bll.DeleteMedarbejderAsync(7);

        _mockRepo.Verify(r => r.DeleteAsync(7), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
