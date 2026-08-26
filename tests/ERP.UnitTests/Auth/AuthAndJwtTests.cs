using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Features.Auth.Commands.Login;
using ERP.Domain.Entities;
using ERP.UnitTests.Common;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace ERP.UnitTests.Auth;

public class AuthAndJwtTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("Admin123!", It.IsAny<string>()))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("fake_jwt_token_sample");

        var handler = new LoginCommandHandler(context, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
        var command = new LoginCommand("admin@erp.com", "Admin123!");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().Be("fake_jwt_token_sample");
        response.Data.User.Email.Should().Be("admin@erp.com");
        response.Data.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowBusinessException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("WrongPassword", It.IsAny<string>()))
            .Returns(false);

        var handler = new LoginCommandHandler(context, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
        var command = new LoginCommand("admin@erp.com", "WrongPassword");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*E-posta adresi veya şifre hatalı*");
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldThrowBusinessException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var handler = new LoginCommandHandler(context, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
        var command = new LoginCommand("nonexistent@erp.com", "SomePass123!");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*E-posta adresi veya şifre hatalı*");
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldThrowBusinessException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user = context.Users.First();
        user.IsActive = false;
        context.SaveChanges();

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("Admin123!", It.IsAny<string>()))
            .Returns(true);

        var handler = new LoginCommandHandler(context, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
        var command = new LoginCommand(user.Email, "Admin123!");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Kullanıcı hesabı aktif değildir*");
    }

    [Theory]
    [InlineData("", "Admin123!")]
    [InlineData("invalid-email", "Admin123!")]
    [InlineData("admin@erp.com", "")]
    [InlineData("admin@erp.com", "123")]
    public void LoginValidator_WithInvalidInputs_ShouldHaveValidationErrors(string email, string password)
    {
        var command = new LoginCommand(email, password);
        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }
}
