# UniversitetApp

Et konsollbasert universitetssystem i C# (.NET 10) for håndtering av innlogging, kurs, brukere og bibliotek.

## Innhold

- **Autentisering:** logg inn eller registrer ny bruker med rolle
- **Kurs:** opprett kurs, meld student på/av kurs, sett karakter, vis deltakere
- **Bibliotek:** registrer bok, lån ut bok, returner bok, søk etter bok
- **Brukertyper:** student, faglærer, bibliotekansatt

## Prosjektstruktur

- `UniversitetApp.sln`: solution-fil
- `UniversitetApp/`: selve app-prosjektet
  - `Program.cs`: oppstart og applikasjonsflyt
  - `Models/`: domeneobjekter (Student, Ansatt, Kurs, Bok, Lån, osv.)
  - `Services/`: forretningslogikk (KursManager, BibliotekManager, AuthService, AppStateStore)
  - `Console/`: menyer og brukerflyt per rolle

## Krav

.NET SDK 10.0+

Sjekk versjon:

```bash
dotnet --version
```

## Bygg og kjør

Fra prosjektroten:

```bash
dotnet build UniversitetApp.sln
dotnet run --project UniversitetApp/UniversitetApp.csproj
```

Evt. hvis du er i `UniversitetApp`-mappa:

```bash
dotnet build
dotnet run
```

## Eksempelbrukere

| Brukernavn | Passord | Rolle            |
|------------|---------|------------------|
| kari       | pass123 | Student          |
| ola        | pass123 | Student          |
| larsen     | pass123 | Faglærer         |
| maja       | pass123 | Bibliotekansatt  |

## Tester

```bash
dotnet test UniversitetApp.sln
```

## Notater

- Tilstand lagres i `data.json` og lastes ved oppstart.
- Hvis filen mangler eller er ugyldig, brukes innebygd eksempeldata.
- Input valideres i service- og modell-lag.
