namespace GiftBase.Features.Gifts;

public static class GiftImageLinkBuilder
{
    public static string Build(int giftId, Guid imageVersion) => $"/gifts/{giftId}/image?v={imageVersion}";
}
