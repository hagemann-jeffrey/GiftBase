namespace GiftBase.Core.Entities;

public class GiftImage
{
    public const long MaxContentLength = 5 * 1024 * 1024;
    public const string JpegContentType = "image/jpeg";
    public const string PngContentType = "image/png";

    private GiftImage() { }

    public GiftImage(int giftId, string contentType, byte[] content)
    {
        GiftId = giftId;
        ContentType = contentType;
        Content = content;
    }

    public int GiftId { get; private set; }
    public string ContentType { get; private set; } = null!;
    public byte[] Content { get; private set; } = null!;

    public static bool IsSupportedContentType(string? contentType) =>
        contentType is JpegContentType or PngContentType;

    public void Replace(string contentType, byte[] content)
    {
        ContentType = contentType;
        Content = content;
    }
}
