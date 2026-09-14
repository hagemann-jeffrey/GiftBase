using GiftBase.Core.Entities;
using GiftBase.Core.Enums;
using GiftBase.Features.GiftSuggestions;
using Shouldly;

namespace GiftBase.Tests;

public class GiftSuggestionPromptTests
{
    private static readonly DateOnly Today = new(2026, 9, 13);

    private static Person BuildPerson(DateOnly? dateOfBirth = null, string? interests = null) =>
        new("John", "Doe", dateOfBirth, Relation.Partner, 1, interests);

    private static Occasion BuildOccasion() =>
        new(OccasionType.Birthday, null, new DateOnly(2026, 5, 24), true, 1);

    [Fact]
    public void BuildInput_ShouldNotContainPersonName()
    {
        // Arrange
        var person = BuildPerson();

        // Act
        var input = GiftSuggestionPrompt.BuildInput(person, BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldNotContain("John");
        input.ShouldNotContain("Doe");
    }

    [Fact]
    public void BuildInput_ShouldContainRelation()
    {
        // Arrange
        var person = BuildPerson();

        // Act
        var input = GiftSuggestionPrompt.BuildInput(person, BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldContain("Beziehung: Partner");
    }

    [Fact]
    public void BuildInput_ShouldContainAge_WhenDateOfBirthIsSet()
    {
        // Arrange
        var person = BuildPerson(new DateOnly(1992, 1, 1));

        // Act
        var input = GiftSuggestionPrompt.BuildInput(person, BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldContain("Alter: 34 Jahre");
    }

    [Fact]
    public void BuildInput_ShouldNotContainAgeLine_WhenDateOfBirthIsNull()
    {
        // Arrange
        var person = BuildPerson();

        // Act
        var input = GiftSuggestionPrompt.BuildInput(person, BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldNotContain("Alter:");
    }

    [Fact]
    public void BuildInput_ShouldContainOccasionDisplayTitle()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldContain("<anlass>\nGeburtstag\n</anlass>");
    }

    [Fact]
    public void BuildInput_ShouldContainBudget()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 75, null, [], [], Today);

        // Assert
        input.ShouldContain("<budget>\n75\n</budget>");
    }

    [Fact]
    public void BuildInput_ShouldContainTags_WhenInterestsAreSet()
    {
        // Arrange
        var person = BuildPerson(interests: "Bücher, Pflanzen");

        // Act
        var input = GiftSuggestionPrompt.BuildInput(person, BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldContain("<tags>\nBücher, Pflanzen\n</tags>");
    }

    [Fact]
    public void BuildInput_ShouldNotContainTagsBlock_WhenInterestsAreNull()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldNotContain("<tags>");
    }

    [Fact]
    public void BuildInput_ShouldContainHint_WhenGiven()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, "Etwas Persönliches", [], [], Today);

        // Assert
        input.ShouldContain("<hinweis>\nEtwas Persönliches\n</hinweis>");
    }

    [Fact]
    public void BuildInput_ShouldNotContainHintBlock_WhenNotGiven()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldNotContain("<hinweis>");
    }

    [Fact]
    public void BuildInput_ShouldContainExistingGiftTitle()
    {
        // Arrange
        var gift = new Gift("Kaffeemaschine", "Für den Morgen", null, 29.99m, 1);

        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, null, [gift], [], Today);

        // Assert
        input.ShouldContain("Kaffeemaschine");
        input.ShouldContain("29.99");
    }

    [Fact]
    public void BuildInput_ShouldContainEmptyArray_WhenNoExistingGifts()
    {
        // Act
        var input = GiftSuggestionPrompt.BuildInput(BuildPerson(), BuildOccasion(), 50, null, [], [], Today);

        // Assert
        input.ShouldContain("<bisherige_geschenke>\n[]\n</bisherige_geschenke>");
    }
}
