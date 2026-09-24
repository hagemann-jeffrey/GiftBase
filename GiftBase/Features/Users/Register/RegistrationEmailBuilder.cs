namespace GiftBase.Features.Users.Register;

public static class RegistrationEmailBuilder
{
    public static (string Subject, string Body) Build(string confirmationLink)
    {
        var body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2>Willkommen bei GiftBase!</h2>
                    <p>Schön, dass du dabei bist. Bitte bestätige deine E-Mail-Adresse, um deinen Account zu aktivieren.</p>
                    <p style='margin-top: 20px;'>
                        <a href='{confirmationLink}'
                           style='background-color: #594AE2; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                           E-Mail bestätigen
                        </a>
                    </p>
                    <p style='margin-top: 30px; font-size: 12px; color: #888;'>
                        Falls der Button nicht funktioniert, kopiere diesen Link in deinen Browser:<br/>
                        {confirmationLink}
                    </p>
                </div>";

        return ("Willkommen bei GiftBase! Bitte bestätige deine E-Mail-Adresse", body);
    }
}
