namespace GiftBase.Core.Entities;

public class GiftSuggestionQuota
{
    public static readonly TimeSpan Window = TimeSpan.FromDays(1);
    public const int MaxRequestsPerWindow = 5;

    private GiftSuggestionQuota() { }

    public GiftSuggestionQuota(int userId)
    {
        UserId = userId;
    }

    public int UserId { get; private set; }
    public int RequestCount { get; private set; }
    public DateTime WindowStartedAt { get; private set; }

    public DateTime ResetsAt => WindowStartedAt + Window;

    public bool IsExhausted(DateTime utcNow) =>
        RequestCount >= MaxRequestsPerWindow && utcNow < ResetsAt;

    public void RegisterRequest(DateTime utcNow)
    {
        if (RequestCount == 0 || utcNow >= ResetsAt)
        {
            WindowStartedAt = utcNow;
            RequestCount = 1;
            return;
        }

        RequestCount++;
    }
}
