using GiftBase.Features.Gifts;
using Shouldly;

namespace GiftBase.Tests;

public class GiftImageLinkBuilderTests
{
    [Fact]
    public void Build_ShouldReturnUrlWithGiftIdAndImageVersion()
    {
        // Arrange
        var imageVersion = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var url = GiftImageLinkBuilder.Build(42, imageVersion);

        // Assert
        url.ShouldBe("/gifts/42/image?v=11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void Build_ShouldReturnDifferentUrls_ForDifferentImageVersions()
    {
        // Act
        var firstUrl = GiftImageLinkBuilder.Build(42, Guid.NewGuid());
        var secondUrl = GiftImageLinkBuilder.Build(42, Guid.NewGuid());

        // Assert
        firstUrl.ShouldNotBe(secondUrl);
    }
}
