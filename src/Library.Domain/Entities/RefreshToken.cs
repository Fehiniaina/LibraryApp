// src/Library.Domain/Entities/RefreshToken.cs
namespace Library.Domain.Entities;

public class RefreshToken
{
    public RefreshToken(string token, string userId, DateTime expiresAt)
    {
        this.Id = Guid.NewGuid();
        this.Token = token;
        this.UserId = userId;
        this.ExpiresAt = expiresAt;
        this.IsRevoked = false;
    }

    protected RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public string Token { get; private set; } = default!;

    public string UserId { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; }

    public bool IsActive => !this.IsRevoked && DateTime.UtcNow < this.ExpiresAt;

    public void Revoke() => this.IsRevoked = true;
}