# Schichtplaner (HostFlow-Planer)

Blazor-Server-Anwendung (.NET 8) zur Schichtplanung für Ferienwohnungs-Teams:
Kalender mit Guesty-Buchungen, Zeiterfassung mit Excel-Export, Team-/Mitarbeiter-
und Tag-Verwaltung, Rollen- und Organisationsmodell.

- UI: MudBlazor + Heron.MudCalendar
- Datenbank: MySQL 8 (EF Core / Pomelo), zwei DbContexts (Fachdaten + Identity),
  die sich dank getrennter Migrations-Verlaufstabellen **eine** Datenbank teilen können
- Anbindung: [Guesty Open API](https://open-api-docs.guesty.com) (OAuth2 Client-ID und Client-Secret werden in den Einstellungen hinterlegt)

## Konfiguration

Alle Werte kommen aus `appsettings.json` bzw. Umgebungsvariablen (empfohlen):

| Umgebungsvariable | Zweck |
|---|---|
| `ConnectionStrings__PlanerContext` | MySQL-Verbindung Fachdaten |
| `ConnectionStrings__PlanerIdentityContext` | MySQL-Verbindung Identity (darf dieselbe DB sein) |
| `MailConfiguration__From` / `__Host` / `__Port` / `__UserName` / `__Password` / `__DisplayName` | SMTP für Bestätigungs-/Passwort-Mails (optional, aber ohne SMTP siehe „Erster Login") |
| `PORT` | Wird vom Hoster (z. B. Railway) gesetzt; Standard 8080 |

Beim Start führt die App automatisch alle EF-Core-Migrationen aus und legt die
Basisrollen an (Employee, Company, Administrator, Developer, Management).

## Deployment auf Railway

1. **Projekt anlegen:** Railway → *New Project* → *Deploy from GitHub repo* →
   dieses Repository wählen. Railway erkennt das `Dockerfile` automatisch.
2. **MySQL hinzufügen:** Im Projekt *Create → Database → MySQL*.
3. **Variablen setzen** (am App-Service, Tab *Variables*), beide mit demselben Wert:

   ```
   ConnectionStrings__PlanerContext =
   Server=${{MySQL.MYSQLHOST}};Port=${{MySQL.MYSQLPORT}};Database=${{MySQL.MYSQLDATABASE}};User=${{MySQL.MYSQLUSER}};Password=${{MySQL.MYSQLPASSWORD}}

   ConnectionStrings__PlanerIdentityContext =
   Server=${{MySQL.MYSQLHOST}};Port=${{MySQL.MYSQLPORT}};Database=${{MySQL.MYSQLDATABASE}};User=${{MySQL.MYSQLUSER}};Password=${{MySQL.MYSQLPASSWORD}}
   ```

   Optional zusätzlich die `MailConfiguration__…`-Variablen für den Mailversand.
4. **Domain erzeugen:** App-Service → *Settings → Networking → Generate Domain*.
5. **Deployen:** Railway baut bei jedem Push auf den verbundenen Branch neu.

### Erster Login

Der **erste registrierte Account** einer frischen Installation wird automatisch
aktiviert und bestätigt und kann sich direkt einloggen.

**Mitarbeiter einladen** funktioniert auch ohne Mailserver: Beim Einladen wird
das Startpasswort einmalig angezeigt, und nicht aktivierte Mitarbeiter können
in der Mitarbeiterliste per Knopfdruck freigeschaltet werden. Mit konfiguriertem
SMTP erhalten Mitarbeiter zusätzlich eine Einladungs-Mail mit Bestätigungslink.

## Lokal entwickeln

Voraussetzungen: .NET 8 SDK, MySQL 8.

```bash
mysql -e "CREATE DATABASE planer CHARACTER SET utf8mb4;"

export ConnectionStrings__PlanerContext="Server=localhost;Database=planer;User=root;Password=..."
export ConnectionStrings__PlanerIdentityContext="Server=localhost;Database=planer;User=root;Password=..."

dotnet run --project planer
```

Die App läuft dann auf http://localhost:5059.
