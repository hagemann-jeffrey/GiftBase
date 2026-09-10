namespace GiftBase.Core.Entities;

public class ShareLink
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    private ShareLink() { }

    public ShareLink(string token, int userId, int personId, int? occasionId, DateTime createdAt)
    {
        Token = token;
        UserId = userId;
        PersonId = personId;
        OccasionId = occasionId;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public string Token { get; private set; } = null!;
    public int UserId { get; private set; }
    public int PersonId { get; private set; }
    public int? OccasionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt => CreatedAt + Lifetime;

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
}
