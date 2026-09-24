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
        gift.ImageVersion.ShouldBeNull();
    }

    [Fact]
    public void Gift_ShouldBeUpdated()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", "Am liebsten in Schwarz", "https://example.com/kaffee", 129.99m, 1);
        var updateDto = new Core.Dtos.Gifts.GiftUpdateDto
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
        gift.Update(new Core.Dtos.Gifts.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Given
        });

        // Assert
        gift.PersonId.ShouldBe(7);
    }

    [Fact]
    public void Gift_ShouldBeCreated_WithOccasion()
    {
        // Act
        var gift = new Gift("Kaffeemaschine", null, null, null, 1, 42);

        // Assert
        gift.OccasionId.ShouldBe(42);
        gift.OccasionLabel.ShouldBeNull();
        gift.OccasionYear.ShouldBeNull();
    }

    [Fact]
    public void Gift_ShouldBeCreated_WithoutOccasion()
    {
        // Act
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);

        // Assert
        gift.OccasionId.ShouldBeNull();
    }

    [Fact]
    public void DetachFromOccasion_ShouldKeepAssignmentAsSnapshot()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1, 42);

        // Act
        gift.DetachFromOccasion("Geburtstag", 2027);

        // Assert
        gift.OccasionId.ShouldBeNull();
        gift.OccasionLabel.ShouldBe("Geburtstag");
        gift.OccasionYear.ShouldBe(2027);
    }

    [Fact]
    public void Gift_ShouldKeepSnapshot_WhenUpdatedWithoutOccasion()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1, 42);
        gift.DetachFromOccasion("Geburtstag", 2027);

        // Act
        gift.Update(new Core.Dtos.Gifts.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Bought,
            OccasionId = null
        });

        // Assert
        gift.OccasionId.ShouldBeNull();
        gift.OccasionLabel.ShouldBe("Geburtstag");
        gift.OccasionYear.ShouldBe(2027);
    }

    [Fact]
    public void Gift_ShouldDropSnapshot_WhenUpdatedWithOccasion()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);
        gift.DetachFromOccasion("Geburtstag", 2027);

        // Act
        gift.Update(new Core.Dtos.Gifts.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Bought,
            OccasionId = 99
        });

        // Assert
        gift.OccasionId.ShouldBe(99);
        gift.OccasionLabel.ShouldBeNull();
        gift.OccasionYear.ShouldBeNull();
    }

    [Fact]
    public void Gift_ShouldAttachImage()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);

        // Act
        gift.AttachImage();

        // Assert
        gift.ImageVersion.ShouldNotBeNull();
    }

    [Fact]
    public void Gift_ShouldChangeImageVersion_WhenImageIsAttachedAgain()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);
        gift.AttachImage();
        var firstVersion = gift.ImageVersion;

        // Act
        gift.AttachImage();

        // Assert
        gift.ImageVersion.ShouldNotBe(firstVersion);
    }

    [Fact]
    public void Gift_ShouldDetachImage()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);
        gift.AttachImage();

        // Act
        gift.DetachImage();

        // Assert
        gift.ImageVersion.ShouldBeNull();
    }

    [Fact]
    public void Gift_ShouldKeepImage_WhenUpdated()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", null, null, null, 1);
        gift.AttachImage();
        var imageVersion = gift.ImageVersion;

        // Act
        gift.Update(new Core.Dtos.Gifts.GiftUpdateDto
        {
            Title = "Espressomaschine",
            Status = GiftStatus.Bought
        });

        // Assert
        gift.ImageVersion.ShouldBe(imageVersion);
    }
}