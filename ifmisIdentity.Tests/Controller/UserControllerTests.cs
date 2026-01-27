

namespace ifmisIdentity.Tests.Controller
{
    using Xunit;
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using ifmisIdentity.Controllers;
    using ifmisIdentity.Models;
    using ifmisIdentity.Data;
    using ifmisIdentity.Dtos;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System;

    public class UserControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<Role>> _mockRoleManager;
        private readonly Mock<IdentityDbContext> _mockDbContext;

        private readonly UserController _controller;

        public UserControllerTests()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<Role>>();
            _mockRoleManager = new Mock<RoleManager<Role>>(roleStore.Object, null, null, null, null);

            var dbContextOptions = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _mockDbContext = new Mock<IdentityDbContext>(dbContextOptions);

            _controller = new UserController(_mockUserManager.Object, _mockRoleManager.Object, _mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var organization = new Organization { Id = 1, Name = "Test Organization" };
            var roles = new List<Role> { new Role { Id = 1, Name = "Admin" } };
            var userRoles = new List<UserRole> { new UserRole { RoleId = 1, Role = roles[0] } };
            var users = new List<User>
        {
            new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Organization = organization,
                UserRoles = userRoles
            }
        };

            var usersDbSet = GetQueryableMockDbSet(users);
            _mockDbContext.Setup(db => db.Users).Returns(usersDbSet.Object);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsType<List<object>>(okResult.Value);
            Assert.Single(returnedUsers);
        }

        [Fact]
        public async Task GetUserById_ReturnsOkResult_WithUser()
        {
            // Arrange
            var organization = new Organization { Id = 1, Name = "Test Organization" };
            var roles = new List<Role> { new Role { Id = 1, Name = "Admin" } };
            var userRoles = new List<UserRole> { new UserRole { RoleId = 1, Role = roles[0] } };
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Organization = organization,
                UserRoles = userRoles
            };

            var usersDbSet = GetQueryableMockDbSet(new List<User> { user });
            _mockDbContext.Setup(db => db.Users).Returns(usersDbSet.Object);

            // Act
            var result = await _controller.GetUserById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<object>(okResult.Value);
        }

        [Fact]
        public async Task CreateUser_ReturnsOkResult_WhenUserCreated()
        {
            // Arrange
            var organization = new Organization { Id = 1, Name = "Test Organization" };
            _mockDbContext.Setup(db => db.Organizations.FindAsync(1)).ReturnsAsync(organization);

            var dto = new UserDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                IsActive = true,
                PasswordHash = "password123",
                OrganizationID =  1 ,
                Roles = new List<string> { "Admin" }
            };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockRoleManager.Setup(rm => rm.RoleExistsAsync("Admin")).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User created successfully.", ((dynamic)okResult.Value).message);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOkResult_WhenUserDeleted()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testuser" };
            _mockUserManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User deleted successfully.", ((dynamic)okResult.Value).message);
        }

        private static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSet;
        }
    }

}
