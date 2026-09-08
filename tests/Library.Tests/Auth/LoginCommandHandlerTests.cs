// tests/Library.Tests/Auth/LoginCommandHandlerTests.cs
using Library.Application.Auth.Commands.Login;
using Library.Domain.Interfaces;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace Library.Tests.Auth;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsGenericErrorMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new LibraryDbContext(options);

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        userManagerMock
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null); // simule "utilisateur introuvable"

        var tokenServiceMock = new Mock<ITokenService>();

        var handler = new LoginCommandHandler(userManagerMock.Object, tokenServiceMock.Object, db);
        var command = new LoginCommand("inexistant@test.com", "motdepasse");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email ou mot de passe incorrect."); // le message générique anti-énumération
        tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()), Times.Never); // confirme qu'on ne génère JAMAIS de token si l'utilisateur n'existe pas
    }
}