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

public class HomeControllerTests
{
    // ── Factory ───────────────────────────────────────────────

    private static HomeController BuildController(
        Mock<IUnitOfWork> uow, string userName = "Simon")
    {
        var vagtBll = new VagtBLL(uow.Object);
        var medBll  = new MedarbejderBLL(uow.Object);

        var identity    = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "Cookie");
        var httpContext  = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var controller = new HomeController(vagtBll, medBll);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static Mock<IUnitOfWork> MakeUoW(List<Vagt>? vagter = null)
    {
        var mockMedRepo  = new Mock<IMedarbejderRepository>();
        var mockVagtRepo = new Mock<IVagtRepository>();

        var simon = new Medarbejder { Id = 1, Name = "Simon", Vagter = new List<Vagt>() };
        mockMedRepo.Setup(r => r.GetMedarbejderByNameAsync(It.IsAny<string>())).ReturnsAsync(simon);
        mockVagtRepo.Setup(r => r.GetVagterByMedarbejderAsync(1))
                    .ReturnsAsync(vagter ?? new List<Vagt>());

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMedRepo.Object);
        uow.Setup(u => u.VagtRepository).Returns(mockVagtRepo.Object);
        return uow;
    }

    // ── Index ─────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        var controller = BuildController(MakeUoW());

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_ViewModelContainsCurrentUsersShifts()
    {
        var friday = DateTime.Today;
        while (friday.DayOfWeek != DayOfWeek.Friday) friday = friday.AddDays(1);

        var vagter = new List<Vagt>
        {
            new Vagt { Id = 1, Dato = friday.AddHours(16), Medarbejdere = new List<Medarbejder>() },
            new Vagt { Id = 2, Dato = friday.AddHours(20), Medarbejdere = new List<Medarbejder>() }
        };

        var controller = BuildController(MakeUoW(vagter));

        var result   = await controller.Index();
        var view     = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomeViewModel>(view.Model);

        Assert.Equal(2, viewModel.MineVagter.Count);
    }

    [Fact]
    public async Task Index_ShiftsAreReturnedInChronologicalOrder()
    {
        var friday = DateTime.Today;
        while (friday.DayOfWeek != DayOfWeek.Friday) friday = friday.AddDays(1);

        // Add in reverse order to verify sorting
        var vagter = new List<Vagt>
        {
            new Vagt { Id = 2, Dato = friday.AddHours(20), Medarbejdere = new List<Medarbejder>() },
            new Vagt { Id = 1, Dato = friday.AddHours(16), Medarbejdere = new List<Medarbejder>() }
        };

        var controller = BuildController(MakeUoW(vagter));

        var result    = await controller.Index();
        var view      = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomeViewModel>(view.Model);

        Assert.True(viewModel.MineVagter[0].Dato < viewModel.MineVagter[1].Dato);
    }

    [Fact]
    public async Task Index_NoShiftsForUser_ViewModelHasEmptyList()
    {
        var controller = BuildController(MakeUoW(new List<Vagt>()));

        var result    = await controller.Index();
        var view      = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomeViewModel>(view.Model);

        Assert.Empty(viewModel.MineVagter);
    }
}
