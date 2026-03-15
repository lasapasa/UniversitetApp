# UniversitetApp

Et konsollbasert universitetssystem i C# (.NET 8) for enkel håndtering av kurs, brukere og bibliotek.

## Innhold

- Kurs: opprett kurs, meld student til kurs, vis kurs og deltakere, søk etter kurs
- Bibliotek: registrer bok, lån ut bok, returner bok, søk etter bok
- Brukertyper: student, utvekslingsstudent, ansatt

## Prosjektstruktur

- UniversitetApp-2.sln: solution-fil
- UniversitetApp/: selve app-prosjektet
  - Program.cs: meny og applikasjonsflyt
  - Models/: domeneobjekter (Student, Kurs, Bok, osv.)
  - Services/: forretningslogikk (KursManager, BibliotekManager)

## Krav

- .NET SDK 8.0+

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

Evt. hvis du er i UniversitetApp-mappa:
```bash
dotnet build
dotnet run
```

## Menyvalg i appen

- [1] Opprett kurs
- [2] Meld student til kurs
- [3] Print kurs og deltakere
- [4] Søk på kurs
- [5] Søk på bok
- [6] Lån bok
- [7] Returner bok
- [8] Registrer bok
- [0] Avslutt

## Notater

- Appen starter med seed-data ved oppstart.
- Input valideres i manager- og modell-lag.
- Modellene bruker innkapsling for tryggere oppdatering av intern tilstand.
