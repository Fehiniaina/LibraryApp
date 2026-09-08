// RefreshTokenCommandHandler.cs
using Library.Application.Auth.Commands.Login;
using Library.Domain.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Identity;
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Auth.Commands.RefreshAccessToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private readonly LibraryDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        LibraryDbContext db,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService
    )
    {
        _db = db;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (storedToken is null || !storedToken.IsActive)
            return new LoginResult(false, null, null, "Refresh token invalide ou expiré.");

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
            return new LoginResult(false, null, null, "Utilisateur introuvable.");

        // Rotation — révoque l'ancien, en émet un nouveau (best practice : usage unique)
        storedToken.Revoke();

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken(newRefreshTokenValue, user.Id, DateTime.UtcNow.AddDays(7));
        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(ct);

        return new LoginResult(true, newAccessToken, newRefreshTokenValue, null);
    }
}