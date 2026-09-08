// src/Library.Domain/Entities/RefreshToken.cs
namespace Library.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = default!;
    public string UserId { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    protected RefreshToken() { }

    public RefreshToken(string token, string userId, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public void Revoke() => IsRevoked = true;
}