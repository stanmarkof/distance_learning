using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using distant.Controllers;
using distant.Data;
using distant.ViewModels;
using distant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Distant.Tests
{
    public class RegTest
    {
        [Fact]
        public async Task Register_ValidModel_ReturnsRedirectToHome()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            using var context = new AppDbContext(options);
            context.AppSettings.Add(new AppSetting { Id = 1, VerificationCode = "12345" });
            context.SaveChanges();

            var userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null);

            var signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null);

            var loggerMock = new Mock<ILogger<AccountController>>();

            var controller = new AccountController(
                userManagerMock.Object,
                signInManagerMock.Object,
                loggerMock.Object,
                context);

            var model = new RegisterViewModel
            {
                UserName = "TestUser",
                Email = "testuser@example.com",
                Password = "Password123!",
                VerificationCode = "12345",
                FirstName = "Test",
                LastName = "User",
                MiddleName = "T"
            };

            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "Student"))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock.Setup(um => um.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()))
                .ReturnsAsync(IdentityResult.Success);

            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

    }
}