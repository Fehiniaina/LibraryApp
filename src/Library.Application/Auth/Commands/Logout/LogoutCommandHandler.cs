// src/Library.Application/Auth/Commands/Logout/LogoutCommandHandler.cs
using Library.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly LibraryDbContext _db;
    public LogoutCommandHandler(LibraryDbContext db) => _db = db;

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken ct)
    {
        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (storedToken is null)
            return new LogoutResult(false, "Refresh token introuvable.");

        storedToken.Revoke();
        await _db.SaveChangesAsync(ct);

        return new LogoutResult(true, null);
    }
}