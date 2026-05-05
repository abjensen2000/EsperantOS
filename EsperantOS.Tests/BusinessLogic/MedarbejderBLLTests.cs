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
        _mockUoW = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IMedarbejderRepository>();
        _mockUoW.Setup(u => u.MedarbejderRepository).Returns(_mockRepo.Object);
        _bll = new MedarbejderBLL(_mockUoW.Object);
    }

    // ── CreateMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task CreateMedarbejder_Test()
    {
        var dto = new MedarbejderDTO { Name = "Emma", Vagter = new List<VagtDTO>() };

        await _bll.CreateMedarbejderAsync(dto);

        _mockRepo.Verify(r => r.AddAsync(It.Is<Medarbejder>(m => m.Name == "Emma")), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── UpdateMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task UpdateMedarbejder_Test()
    {
        var dto = new MedarbejderDTO { Id = 1, Name = "Simon", Bestyrelsesmedlem = true, Vagter = new List<VagtDTO>() };

        await _bll.UpdateMedarbejderAsync(dto);

        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Medarbejder>(m => m.Id == 1 && m.Name == "Simon")), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── DeleteMedarbejderAsync ────────────────────────────────

    [Fact]
    public async Task DeleteMedarbejder_Test()
    {
        await _bll.DeleteMedarbejderAsync(7);

        _mockRepo.Verify(r => r.DeleteAsync(7), Times.Once);
        _mockUoW.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}