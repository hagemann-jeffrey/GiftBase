using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using Shouldly;

namespace GiftBase.Tests;

public class GiftTests
{
    [Fact]
    public void Gift_ShouldBeCreated()
    {
        // Act
        var gift = new Gift("Kaffeemaschine", "Am liebsten in Schwarz", "https://example.com/kaffee", 129.99m, 1);

        // Assert
        gift.Title.ShouldBe("Kaffeemaschine");
        gift.Note.ShouldBe("Am liebsten in Schwarz");
        gift.Link.ShouldBe("https://example.com/kaffee");
        gift.Price.ShouldBe(129.99m);
        gift.Status.ShouldBe(GiftStatus.Idea);
        gift.PersonId.ShouldBe(1);
    }

    [Fact]
    public void Gift_ShouldBeCreated_WithoutOptionalValues()
    {
        // Act
        var gift = new Gift("Gutschein", null, null, null, 1);

        // Assert
        gift.Title.ShouldBe("Gutschein");
        gift.Note.ShouldBeNull();
        gift.Link.ShouldBeNull();
        gift.Price.ShouldBeNull();
    }

    [Fact]
    public void Gift_ShouldBeUpdated()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", "Am liebsten in Schwarz", "https://example.com/kaffee", 129.99m, 1);
        var updateDto = new Core.Dtos.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Note = null,
            Link = "https://example.com/espresso",
            Price = 249.50m,
            Status = GiftStatus.Bought
        };

        // Act
        gift.Update(updateDto);

        // Assert
        gift.Title.ShouldBe("Espressomaschine");
        gift.Note.ShouldBeNull();
        gift.Link.ShouldBe("https://example.com/espresso");
        gift.Price.ShouldBe(249.50m);
        gift.Status.ShouldBe(GiftStatus.Bought);
    }

    [Fact]
    public void Gift_ShouldNotChangePersonId_WhenUpdated()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 7);

        // Act
        gift.Update(new Core.Dtos.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Given
        });

        // Assert
        gift.PersonId.ShouldBe(7);
    }
}