using GiftBase.Shared;
using Shouldly;

namespace GiftBase.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("  Eine Notiz  ", "Eine Notiz")]
    [InlineData("Eine Notiz", "Eine Notiz")]
    [InlineData("\tEine Notiz\n", "Eine Notiz")]
    public void NormalizeOptional_ShouldRemoveSurroundingWhitespace_WhenValueHasContent(string value, string expected)
    {
        // Act
        var result = value.NormalizeOptional();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void NormalizeOptional_ShouldReturnNull_WhenValueIsBlank(string? value)
    {
        // Act
        var result = value.NormalizeOptional();

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("  John  ", "John")]
    [InlineData("John", "John")]
    [InlineData("\tJohn\n", "John")]
    public void NormalizeRequired_ShouldRemoveSurroundingWhitespace(string value, string expected)
    {
        // Act
        var result = value.NormalizeRequired();

        // Assert
        result.ShouldBe(expected);
    }
}
