using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using distant.Controllers;
using distant.Data;
using distant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Distant.Tests
{
    public class ManageTest
    {
        [Fact]
        public async Task ManageUsers_ReturnsUsers()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            using var context = new AppDbContext(options); // Реальный AppDbContext
            var loggerMock = new Mock<ILogger<AdminController>>();

            // Создаем экземпляры мока UserManager и RoleManager
            var userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null);

            var roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(
                new Mock<IRoleStore<IdentityRole<int>>>().Object,
                null, null, null, null);

            var controller = new AdminController(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                loggerMock.Object);

            // Act
            var result = await controller.ManageUsers(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }
    }
}