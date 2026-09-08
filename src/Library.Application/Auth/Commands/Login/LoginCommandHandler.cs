// LoginCommandHandler.cs
using Library.Domain.Entities;
using Library.Domain.Interfaces;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Library.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly LibraryDbContext _db;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, LibraryDbContext db)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new LoginResult(false, null, null, "Email ou mot de passe incorrect.");

        // CheckPasswordAsync gère la vérification du hash + le lockout automatiquement
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return new LoginResult(false, null, null, "Email ou mot de passe incorrect.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(refreshTokenValue, user.Id, DateTime.UtcNow.AddDays(7));
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return new LoginResult(true, accessToken, refreshTokenValue, null);
    }
}