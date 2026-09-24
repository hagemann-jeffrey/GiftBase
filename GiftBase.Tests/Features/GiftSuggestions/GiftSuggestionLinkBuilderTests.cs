using GiftBase.Core.Enums;
using GiftBase.Features.GiftSuggestions;
using Shouldly;

namespace GiftBase.Tests.Features.GiftSuggestions;

public class GiftSuggestionLinkBuilderTests
{
    [Fact]
    public void Build_ShouldReturnAmazonSearchLink_WhenTypeIsProduct()
    {
        // Act
        var link = GiftSuggestionLinkBuilder.Build(GiftSuggestionType.Product, "Kopfhörer");

        // Assert
        link.ShouldBe("https://www.amazon.de/s?k=Kopfh%C3%B6rer");
    }

    [Fact]
    public void Build_ShouldReturnGoogleSearchLink_WhenTypeIsService()
    {
        // Act
        var link = GiftSuggestionLinkBuilder.Build(GiftSuggestionType.Service, "Kochkurs Berlin");

        // Assert
        link.ShouldBe("https://www.google.com/search?q=Kochkurs%20Berlin");
    }

    [Fact]
    public void Build_ShouldReturnNull_WhenTypeIsMoney()
    {
        // Act
        var link = GiftSuggestionLinkBuilder.Build(GiftSuggestionType.Money, "Geldgeschenk");

        // Assert
        link.ShouldBeNull();
    }
}
