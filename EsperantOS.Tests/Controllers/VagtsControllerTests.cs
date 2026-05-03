using System.Security.Claims;
using EsperantOS.BusinessLogic;
using EsperantOS.Controllers;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DTO.Model;
using EsperantOS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EsperantOS.Tests.Controllers;

public class VagtsControllerTests
{
    // ── Factories ─────────────────────────────────────────────

    private static VagtsController BuildController(Mock<IUnitOfWork> uow, string userName = "Simon")
    {
        var vagtBll = new VagtBLL(uow.Object);
        var medBll  = new MedarbejderBLL(uow.Object);

        var identity   = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "Cookie");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var controller = new VagtsController(vagtBll, medBll);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static DateTime NextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    /// <summary>Builds a UoW mock pre-wired with common defaults.</summary>
    private static Mock<IUnitOfWork> MakeUoW(
        List<Vagt>? fridayVagter   = null,
        Vagt?       vagtById       = null,
        Medarbejder? medarbejder   = null)
    {
        var mockVagtRepo = new Mock<IVagtRepository>();
        var mockMedRepo  = new Mock<IMedarbejderRepository>();

        // EnsureAedruVagterAsync always needs GetFridayVagterAsync
        mockVagtRepo.Setup(r => r.GetFridayVagterAsync())
                    .ReturnsAsync(fridayVagter ?? new List<Vagt>());

        if (vagtById != null)
            mockVagtRepo.Setup(r => r.GetVagtWithMedarbejdereAsync(vagtById.Id))
                        .ReturnsAsync(vagtById);

        if (medarbejder != null)
            mockMedRepo.Setup(r => r.GetMedarbejderByNameAsync(medarbejder.Name))
                       .ReturnsAsync(medarbejder);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.VagtRepository).Returns(mockVagtRepo.Object);
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMedRepo.Object);
        return uow;
    }

    private static Vagt MakeVagt(int id, bool frigivet = false, bool ædru = false) =>
        new Vagt
        {
            Id           = id,
            Dato         = NextFriday().AddHours(20),
            Frigivet     = frigivet,
            Ædru         = ædru,
            Medarbejdere = new List<Medarbejder>()
        };

    // ── Index ─────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewWithSortedVagter()
    {
        var friday = NextFriday();
        var vagter = new List<Vagt>
        {
            MakeVagt(2), // deliberately unsorted
            MakeVagt(1)
        };
        vagter[0].Dato = friday.AddHours(20);
        vagter[1].Dato = friday.AddHours(16);

        var uow        = MakeUoW(fridayVagter: vagter);
        var controller = BuildController(uow);

        var result = await controller.Index();

        var view  = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<Vagt>>(view.Model);
        Assert.Equal(2, model.Count);
        Assert.True(model[0].Dato <= model[1].Dato); // sorted ascending
    }

    // ── Details ───────────────────────────────────────────────

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        var controller = BuildController(MakeUoW());
        var result     = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_NonExistingId_ReturnsNotFound()
    {
        var uow        = MakeUoW(); // nothing wired for id 99
        var controller = BuildController(uow);
        var result     = await controller.Details(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ExistingId_ReturnsViewWithVagt()
    {
        var vagt       = MakeVagt(5);
        var uow        = MakeUoW(vagtById: vagt);
        var controller = BuildController(uow);

        var result = await controller.Details(5);

        var view  = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Vagt>(view.Model);
        Assert.Equal(5, model.Id);
    }

    // ── Create GET ────────────────────────────────────────────

    [Fact]
    public async Task Create_Get_PopulatesViewBagAndReturnsView()
    {
        var controller = BuildController(MakeUoW());

        var result = controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(controller.ViewBag.UpcomingFridays);
        Assert.NotNull(controller.ViewBag.Times);
    }

    // ── Create POST ───────────────────────────────────────────

    [Fact]
    public async Task Create_Post_NonFriday_DoesNotCreate()
    {
        var uow        = MakeUoW();
        var controller = BuildController(uow);

        var monday = DateTime.Today;
        while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(1);

        var result = await controller.Create(monday, "16:00:00", false, false);

        // Should redirect back to Create (not Index) due to model error
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Create", redirect.ActionName);
        uow.Object.VagtRepository.GetType(); // ensure Add was never called
    }

    [Fact]
    public async Task Create_Post_ValidFriday_CreatesAndRedirectsToIndex()
    {
        var uow        = MakeUoW();
        var controller = BuildController(uow);
        var friday     = NextFriday();

        var result = await controller.Create(friday, "20:00:00", false, true);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── Edit GET ──────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_NullId_ReturnsNotFound()
    {
        var result = await BuildController(MakeUoW()).Edit((int?)null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_NonExistingId_ReturnsNotFound()
    {
        var result = await BuildController(MakeUoW()).Edit(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ExistingId_ReturnsViewAndPopulatesViewBag()
    {
        var vagt       = MakeVagt(3);
        var controller = BuildController(MakeUoW(vagtById: vagt));

        var result = await controller.Edit(3);

        Assert.IsType<ViewResult>(result);
        Assert.NotNull(controller.ViewBag.UpcomingFridays);
        Assert.NotNull(controller.ViewBag.Times);
    }

    // ── Edit POST ─────────────────────────────────────────────

    [Fact]
    public async Task Edit_Post_NonExistingId_ReturnsNotFound()
    {
        var controller = BuildController(MakeUoW());

        var result = await controller.Edit(99, NextFriday(), "20:00:00", false, false);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_NonFriday_RedirectsBackToEdit()
    {
        var vagt       = MakeVagt(3);
        var controller = BuildController(MakeUoW(vagtById: vagt));

        var monday = DateTime.Today;
        while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(1);

        var result = await controller.Edit(3, monday, "16:00:00", false, false);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Post_ValidFriday_UpdatesAndRedirectsToIndex()
    {
        var vagt = MakeVagt(3);
        var uow  = MakeUoW(vagtById: vagt);
        uow.Setup(u => u.VagtRepository.GetVagtWithMedarbejdereAsync(3)).ReturnsAsync(vagt);

        var controller = BuildController(uow);

        var result = await controller.Edit(3, NextFriday(), "20:00:00", true, false);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── Frigiv ────────────────────────────────────────────────

    [Fact]
    public async Task Frigiv_NonExistingId_ReturnsNotFound()
    {
        var result = await BuildController(MakeUoW()).Frigiv(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Frigiv_ExistingId_SetsFrigivetTrueAndReturnsOk()
    {
        var vagt = MakeVagt(1, frigivet: false);
        var uow  = MakeUoW(vagtById: vagt);
        uow.Setup(u => u.VagtRepository.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(vagt);

        var result = await BuildController(uow).Frigiv(1);

        Assert.IsType<OkResult>(result);
        Assert.True(vagt.Frigivet);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ── TagVagt ───────────────────────────────────────────────

    [Fact]
    public async Task TagVagt_NonExistingId_ReturnsNotFound()
    {
        var result = await BuildController(MakeUoW()).TagVagt(99);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task TagVagt_KnownUser_AssignsUserToVagtAndReturnsOk()
    {
        var vagt  = MakeVagt(1, frigivet: true);
        var simon = new Medarbejder { Id = 1, Name = "Simon", Vagter = new List<Vagt>() };

        var mockMed  = new Mock<IMedarbejderRepository>();
        var mockVagt = new Mock<IVagtRepository>();

        // First call by TagVagt, second call inside UpdateVagtAsync
        mockMed.Setup(r => r.GetMedarbejderByNameAsync("Simon")).ReturnsAsync(simon);
        mockMed.Setup(r => r.GetByIdAsync(simon.Id)).ReturnsAsync(simon);

        mockVagt.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(new List<Vagt>());
        mockVagt.Setup(r => r.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(vagt);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.VagtRepository).Returns(mockVagt.Object);
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMed.Object);

        var result = await BuildController(uow, userName: "Simon").TagVagt(1);

        Assert.IsType<OkResult>(result);
        Assert.False(vagt.Frigivet);
        Assert.Single(vagt.Medarbejdere);
        Assert.Equal("Simon", vagt.Medarbejdere[0].Name);
    }

    [Fact]
    public async Task TagVagt_UnknownUser_AutoCreatesEmployeeAndAssigns()
    {
        var vagt    = MakeVagt(1, frigivet: true);
        var mockMed = new Mock<IMedarbejderRepository>();
        var newUser = new Medarbejder { Id = 99, Name = "NyBruger", Vagter = new List<Vagt>() };

        // First lookup returns null; second (after create) returns the new user
        mockMed.SetupSequence(r => r.GetMedarbejderByNameAsync("NyBruger"))
               .ReturnsAsync((Medarbejder?)null)
               .ReturnsAsync(newUser);

        // UpdateVagtAsync fetches each medarbejder by id from MedarbejderRepository
        mockMed.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(newUser);

        var mockVagt = new Mock<IVagtRepository>();
        mockVagt.Setup(r => r.GetFridayVagterAsync()).ReturnsAsync(new List<Vagt>());
        mockVagt.Setup(r => r.GetVagtWithMedarbejdereAsync(1)).ReturnsAsync(vagt);

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.VagtRepository).Returns(mockVagt.Object);
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMed.Object);

        var result = await BuildController(uow, userName: "NyBruger").TagVagt(1);

        Assert.IsType<OkResult>(result);
        mockMed.Verify(r => r.AddAsync(It.Is<Medarbejder>(m => m.Name == "NyBruger")), Times.Once);
    }

    // ── Delete GET ────────────────────────────────────────────

    [Fact]
    public async Task Delete_Get_NullId_ReturnsNotFound()
    {
        var result = await BuildController(MakeUoW()).Delete(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ExistingId_ReturnsView()
    {
        var vagt   = MakeVagt(5);
        var result = await BuildController(MakeUoW(vagtById: vagt)).Delete(5);
        var view   = Assert.IsType<ViewResult>(result);
        Assert.IsType<Vagt>(view.Model);
    }

    // ── DeleteConfirmed POST ──────────────────────────────────

    [Fact]
    public async Task DeleteConfirmed_ExistingId_DeletesAndRedirectsToIndex()
    {
        var vagt = MakeVagt(5);
        var uow  = MakeUoW(vagtById: vagt);
        uow.Setup(u => u.VagtRepository.GetVagtWithMedarbejdereAsync(5)).ReturnsAsync(vagt);

        var result = await BuildController(uow).DeleteConfirmed(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_NonExistingId_RedirectsWithoutDeleting()
    {
        var uow    = MakeUoW(); // id 99 not wired
        var result = await BuildController(uow).DeleteConfirmed(99);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
