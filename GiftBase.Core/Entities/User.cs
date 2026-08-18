namespace GiftBase.Core.Entities;

public class User
{
    private User() { }

    public User(string email, string? verificationToken = null, DateTime? tokenExpiresAt = null)
    {
        Email = email;
        IsEmailVerified = false;
        VerificationToken = verificationToken;
        TokenExpiresAt = tokenExpiresAt;
    }

    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public bool IsEmailVerified { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public string? VerificationToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void ConfirmEmail()
    {
        IsEmailVerified = true;
        VerificationToken = null;
        TokenExpiresAt = null;
    }
}
