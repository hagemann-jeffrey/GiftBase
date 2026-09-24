using GiftBase.Core.Entities;
using Shouldly;

namespace GiftBase.Tests.Features.Gifts;

public class GiftImageTests
{
    [Fact]
    public void GiftImage_ShouldBeCreated()
    {
        // Arrange
        byte[] content = [0xFF, 0xD8, 0xFF];

        // Act
        var giftImage = new GiftImage(1, GiftImage.JpegContentType, content);

        // Assert
        giftImage.GiftId.ShouldBe(1);
        giftImage.ContentType.ShouldBe(GiftImage.JpegContentType);
        giftImage.Content.ShouldBe(content);
    }

    [Fact]
    public void GiftImage_ShouldBeReplaced()
    {
        // Arrange
        var giftImage = new GiftImage(1, GiftImage.JpegContentType, [0xFF, 0xD8, 0xFF]);
        byte[] newContent = [0x89, 0x50, 0x4E, 0x47];

        // Act
        giftImage.Replace(GiftImage.PngContentType, newContent);

        // Assert
        giftImage.ContentType.ShouldBe(GiftImage.PngContentType);
        giftImage.Content.ShouldBe(newContent);
    }

    [Theory]
    [InlineData(GiftImage.JpegContentType, true)]
    [InlineData(GiftImage.PngContentType, true)]
    [InlineData("application/pdf", false)]
    [InlineData(null, false)]
    public void IsSupportedContentType_ShouldValidateAgainstAllowlist(string? contentType, bool expected)
    {
        // Act
        var result = GiftImage.IsSupportedContentType(contentType);

        // Assert
        result.ShouldBe(expected);
    }
}
