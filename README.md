 # GiftBase

Webanwendung zum Verwalten von Geschenkideen. GiftBase hält fest, wem man wann etwas schenken
möchte, sammelt Ideen dazu und erinnert rechtzeitig per E-Mail — damit Geburtstage und
Weihnachten nicht zur Last-Minute-Suche werden.

Die Registrierung ist auf die Hochschuldomains `iu-study.org` und `iu.org` beschränkt.

## Funktionsumfang

- **Personen** mit Beziehung (Familie, Freunde, Kolleg:innen, Partner:in, Nachbarschaft,
  Sonstige), Geburtsdatum und Interessen
- **Anlässe** je Person: Geburtstag, Weihnachten oder frei definierte Termine, wiederkehrend
  berechnet
- **Geschenkideen** mit Status von *Idee* über *Gekauft* und *Verpackt* bis *Verschenkt*,
  optional mit Bild (JPEG oder PNG, bis 5 MB)
- **KI-Vorschläge** auf Basis der hinterlegten Interessen über die Google-Gemini-API,
  unterschieden nach Produkt, Dienstleistung und Geldgeschenk — begrenzt auf fünf Anfragen je
  Benutzer und Tag
- **Teilen** einer Wunschliste über einen Link, der nach 30 Tagen abläuft
- **E-Mail-Erinnerungen**: zu Monatsbeginn eine Übersicht der Anlässe des Folgemonats, dazu in der
  Weihnachtszeit eine wöchentliche Erinnerung
- **Registrierung** mit Bestätigung der E-Mail-Adresse, Anmeldung über Cookie-Authentifizierung

Die Oberfläche ist durchgängig deutsch (`de-DE`, Zeitzone `Europe/Berlin`).

## Technologie

| Bereich | Einsatz |
| --- | --- |
| Laufzeit | .NET 10 |
| Oberfläche | Blazor Server (Interactive Server Rendering), MudBlazor 9.9 |
| Datenzugriff | EF Core 10, SQL Server |
| E-Mail | MailKit 4.17 über SMTP mit STARTTLS |
| KI | Google.GenAI 1.21 (Gemini) |
| Tests | xUnit, Shouldly, NSubstitute, EF Core InMemory |

## Lokale Entwicklung

### Voraussetzungen

- .NET SDK 10
- Eine erreichbare SQL-Server-Instanz (lokal oder als Container)
- `dotnet-ef` für Migrationen: `dotnet tool install --global dotnet-ef`
- Ein Gmail-Konto mit App-Passwort sowie ein Gemini-API-Schlüssel, falls E-Mail-Versand und
  Geschenkvorschläge getestet werden sollen

### Konfiguration

Die Anwendung erwartet ihre Einstellungen in `GiftBase/appsettings.Development.json`. Die Datei ist
über `.gitignore` von der Versionsverwaltung ausgeschlossen und muss lokal nach diesem Muster
angelegt werden:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "GiftBase": "Data Source=localhost;Initial Catalog=giftbase;User ID=sa;Password=<PASSWORT>;Encrypt=True;Trust Server Certificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "<SMTP-SERVER>",
    "SmtpPort": "<SMTP-PORT>",
    "SmtpUsername": "<ABSENDERADRESSE>",
    "SmtpPassword": "<SMTP-PASSWORD>",
    "SenderName": "GiftBase Team",
    "SenderEmail": "<ABSENDERADRESSE>"
  },
  "Gemini": {
    "ApiKey": "<GEMINI-API-SCHLÜSSEL>"
  },
  "Notifications": {
    "TriggerSecret": "local-dev-secret"
  }
}
```

Alle Pflichtwerte werden beim Start geprüft (`ValidateOnStart`). Fehlt einer, bricht die Anwendung
mit einer aussagekräftigen Meldung ab, statt später unbemerkt fehlzuschlagen. Das Modell für die
Gemini-Anfragen steht in `appsettings.json` und gilt für alle Umgebungen.

### Starten

```bash
dotnet run --project GiftBase/GiftBase.csproj
```

Ausstehende Migrationen werden beim Start automatisch angewendet — ein separater Schritt zur
Einrichtung der Datenbank ist nicht nötig, die Datenbank selbst muss aber existieren.

### Tests

```bash
dotnet test --configuration Release
```

### Migrationen

```bash
dotnet ef migrations add <Name> --project GiftBase.Data --startup-project GiftBase
```

## Projektstruktur

| Projekt | Inhalt |
| --- | --- |
| `GiftBase` | Blazor-Oberfläche und Anwendungslogik, organisiert in Feature-Ordnern unter `Features/` |
| `GiftBase.Core` | Domänenschicht: Entitäten, DTOs, Enums, Ausnahmen, Schnittstellen — ohne externe Abhängigkeiten |
| `GiftBase.Data` | EF-Core-Kontext, Entitätskonfigurationen und Migrationen |
| `GiftBase.Tests` | xUnit-Tests, in der Ordnerstruktur den Feature-Ordnern nachgebildet |

Konventionen zu Aufbau und Codestil sind in [CLAUDE.md](CLAUDE.md) festgehalten und werden über
`.editorconfig` beim Bauen erzwungen.

## Betrieb

Die Anwendung läuft in Azure App Service und wird bei jedem Push auf `main` über GitHub Actions
ausgerollt (`.github/workflows/deploy.yml`). Der tägliche Versand der Erinnerungen wird von
`.github/workflows/notifications.yml` angestoßen.

## Lizenz

Copyright (c) 2026 Jeffrey Hagemann. Alle Rechte vorbehalten. Siehe [LICENSE](LICENSE).
Hinweise zu verwendeten Drittanbieter-Komponenten in
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
