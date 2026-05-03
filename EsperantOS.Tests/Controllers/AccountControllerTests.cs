using System.Security.Claims;
using EsperantOS.BusinessLogic;
using EsperantOS.Controllers;
using EsperantOS.DataAccess.Repositories;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.DTO.Model;
using EsperantOS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EsperantOS.Tests.Controllers;

public class AccountControllerTests
{
    // ── Factory ───────────────────────────────────────────────

    private static (AccountController controller, Mock<IAuthenticationService> authSvc)
        BuildController(Mock<IUnitOfWork> uow, bool authenticated = false)
    {
        var authSvc = new Mock<IAuthenticationService>();
        authSvc.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(), It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);
        authSvc.Setup(a => a.SignOutAsync(
                It.IsAny<HttpContext>(), It.IsAny<string?>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        var urlHelperFactory = new Mock<IUrlHelperFactory>();
        urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                        .Returns(new Mock<IUrlHelper>().Object);

        var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
                       .Returns(new Mock<ITempDataDictionary>().Object);

        var services = new ServiceCollection();
        services.AddSingleton(authSvc.Object);
        services.AddSingleton(urlHelperFactory.Object);
        services.AddSingleton(tempDataFactory.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        if (authenticated)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "Simon") }, "Cookie");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        var medBll     = new MedarbejderBLL(uow.Object);
        var controller = new AccountController(medBll);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return (controller, authSvc);
    }

    private static Mock<IUnitOfWork> MakeUoW(MedarbejderDTO? dbUser = null)
    {
        var mockMedRepo = new Mock<IMedarbejderRepository>();
        mockMedRepo
            .Setup(r => r.GetMedarbejderByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(dbUser == null ? null
                : new Medarbejder { Id = dbUser.Id, Name = dbUser.Name, Vagter = new List<Vagt>() });

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMedRepo.Object);
        return uow;
    }

    // ── Login GET ─────────────────────────────────────────────

    [Fact]
    public void Login_Get_AlreadyAuthenticated_RedirectsToHome()
    {
        var (controller, _) = BuildController(MakeUoW(), authenticated: true);

        var result = controller.Login();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index",  redirect.ActionName);
        Assert.Equal("Home",   redirect.ControllerName);
    }

    [Fact]
    public void Login_Get_NotAuthenticated_ReturnsLoginView()
    {
        var (controller, _) = BuildController(MakeUoW(), authenticated: false);

        var result = controller.Login();

        Assert.IsType<ViewResult>(result);
    }

    // ── Login POST ────────────────────────────────────────────

    [Fact]
    public async Task Login_Post_ValidCredentials_SignsInAndRedirectsToHome()
    {
        var dbUser = new MedarbejderDTO { Id = 1, Name = "Simon" };
        var (controller, authSvc) = BuildController(MakeUoW(dbUser));
        var model = new LoginViewModel { Username = "simon", Password = "test123" };

        var result = await controller.Login(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home",  redirect.ControllerName);
        authSvc.Verify(a => a.SignInAsync(
            It.IsAny<HttpContext>(), It.IsAny<string?>(),
            It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()), Times.Once);
    }

    [Theory]
    [InlineData("Simon", "test123")]  // correct case also accepted
    [InlineData("SIMON", "test123")]  // all-caps accepted
    public async Task Login_Post_CaseInsensitiveUsername_Succeeds(string username, string password)
    {
        var dbUser = new MedarbejderDTO { Id = 1, Name = "Simon" };
        var (controller, _) = BuildController(MakeUoW(dbUser));
        var model = new LoginViewModel { Username = username, Password = password };

        var result = await controller.Login(model);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Login_Post_WrongPassword_ReturnsViewWithModelError()
    {
        var (controller, authSvc) = BuildController(MakeUoW());
        var model = new LoginViewModel { Username = "simon", Password = "wrong" };

        var result = await controller.Login(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        authSvc.Verify(a => a.SignInAsync(
            It.IsAny<HttpContext>(), It.IsAny<string?>(),
            It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()), Times.Never);
    }

    [Fact]
    public async Task Login_Post_WrongUsername_ReturnsViewWithModelError()
    {
        var (controller, _) = BuildController(MakeUoW());
        var model = new LoginViewModel { Username = "notSimon", Password = "test123" };

        var result = await controller.Login(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_StoresDbNameInClaim()
    {
        // DB has "Simon" with capital S; user types "simon" — claim must use the DB name
        var dbUser = new MedarbejderDTO { Id = 1, Name = "Simon" };
        ClaimsPrincipal? capturedPrincipal = null;

        var authSvc = new Mock<IAuthenticationService>();
        authSvc.Setup(a => a.SignInAsync(
                It.IsAny<HttpContext>(), It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()))
            .Callback<HttpContext, string?, ClaimsPrincipal, AuthenticationProperties?>(
                (_, _, p, _) => capturedPrincipal = p)
            .Returns(Task.CompletedTask);

        var urlHelperFactory2 = new Mock<IUrlHelperFactory>();
        urlHelperFactory2.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                         .Returns(new Mock<IUrlHelper>().Object);
        var tempDataFactory2 = new Mock<ITempDataDictionaryFactory>();
        tempDataFactory2.Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
                        .Returns(new Mock<ITempDataDictionary>().Object);

        var services = new ServiceCollection();
        services.AddSingleton(authSvc.Object);
        services.AddSingleton(urlHelperFactory2.Object);
        services.AddSingleton(tempDataFactory2.Object);
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var mockMedRepo = new Mock<IMedarbejderRepository>();
        mockMedRepo.Setup(r => r.GetMedarbejderByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new Medarbejder { Id = 1, Name = "Simon", Vagter = new List<Vagt>() });

        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.MedarbejderRepository).Returns(mockMedRepo.Object);

        var bll        = new MedarbejderBLL(uow.Object);
        var controller = new AccountController(bll);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        await controller.Login(new LoginViewModel { Username = "simon", Password = "test123" });

        Assert.NotNull(capturedPrincipal);
        var nameClaim = capturedPrincipal!.FindFirst(ClaimTypes.Name);
        Assert.Equal("Simon", nameClaim?.Value); // must be DB name, not typed name
    }

    // ── Logout ────────────────────────────────────────────────

    [Fact]
    public async Task Logout_SignsOutAndRedirectsToLogin()
    {
        var (controller, authSvc) = BuildController(MakeUoW());

        var result = await controller.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        authSvc.Verify(a => a.SignOutAsync(
            It.IsAny<HttpContext>(), It.IsAny<string?>(),
            It.IsAny<AuthenticationProperties?>()), Times.Once);
    }
}
