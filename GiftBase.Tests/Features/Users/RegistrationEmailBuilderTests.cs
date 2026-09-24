using GiftBase.Features.Users.Register;
using Shouldly;

namespace GiftBase.Tests.Features.Users;

public class RegistrationEmailBuilderTests
{
    [Fact]
    public void Build_ShouldIncludeConfirmationLinkAndSubject()
    {
        // Arrange
        var confirmationLink = "https://giftbase.example/confirmemail?Token=abc123";

        // Act
        var (subject, body) = RegistrationEmailBuilder.Build(confirmationLink);

        // Assert
        subject.ShouldBe("Willkommen bei GiftBase! Bitte bestätige deine E-Mail-Adresse");
        body.ShouldContain(confirmationLink);
        body.ShouldContain("E-Mail bestätigen");
    }
}
