using GiftBase.Core.Interfaces;

namespace GiftBase.Features.User.Register;

public partial class Register(ILogger<Register> logger, IAuthService authService)
{
    private RegisterInput RegisterInput { get; set; } = new RegisterInput();

    private async Task RegisterSubmit()
    {
        logger.LogInformation("Registering user with email: {Email}", RegisterInput.Email);

        var success = await authService.RegisterUserAsync(RegisterInput.Email, RegisterInput.Password);
        if (success)
        {
            logger.LogInformation("User registered successfully.");
        }
        else
        {
            logger.LogError("Failed to register user.");
        }
    }
}