using VirtualQueueApi.Models.Entities;

namespace VirtualQueueApi.Domain.Entities;

public sealed class RefreshToken : EntityBase<int>
{
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Invalidated { get; set; }
    public bool Used { get; set; }

    public Guid JwtId { get; set; } = Guid.Empty;
    public int UserId { get; set; }
    public User User { get; set; }

    public RefreshToken() { }

    public RefreshToken(string tokenHash, Guid jti, User user)
    {
        JwtId = jti;        
        User = user;
        TokenHash = tokenHash;
        ExpiresAt = DateTime.UtcNow.AddDays(7);
    }
}
