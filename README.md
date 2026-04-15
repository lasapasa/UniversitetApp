# UniversitetApp

UniversitetApp er en konsollapplikasjon i C# (.NET 10) som simulerer et enkelt universitetssystem.
Systemet håndterer innlogging, kursadministrasjon og bibliotekfunksjoner for ulike brukerroller.

## Hva prosjektet gjør

Applikasjonen lar brukere logge inn eller registrere seg, og viser deretter en meny basert på rolle.

- Studenter kan melde seg på og av kurs, se egne kurs og karakterer, samt søke, låne og returnere bøker.
- Faglærere kan opprette kurs, sette karakterer i egne kurs og registrere pensum.
- Bibliotekansatte kan registrere bøker og se aktive lån og historikk.

## Hvordan prosjektet er bygget

Prosjektet er delt i tydelige lag:

- Console: menyer og brukerflyt.
- Models: domenemodeller for brukere, kurs, bøker og lån.
- Services: forretningslogikk for autentisering, kurs og bibliotek.
- Common: felles responsmodell for suksess/feil.

Arkitekturen gjør det enklere å holde UI, domene og logikk adskilt.

## Data og lagring

Systemet bruker data.json for enkel persistens.

- Ved oppstart forsøker appen å laste data fra fil.
- Hvis fil ikke finnes eller ikke kan leses, brukes eksempeldata.
- Ved avslutning lagres oppdatert tilstand tilbake til fil.

## Kjøring av prosjektet

Krav: .NET SDK 10.0 eller nyere.

Bygg og kjør fra prosjektrot:

```bash
dotnet build UniversitetApp.sln
dotnet run --project UniversitetApp/UniversitetApp.csproj
```

## Tester

Prosjektet inneholder et eget testprosjekt med enhetstester for sentrale regler.

Kjør tester:

```bash
dotnet test UniversitetApp.sln
```

## Eksempelbrukere

- Student: kari / pass123
- Student: ola / pass123
- Faglærer: larsen / pass123
- Bibliotekansatt: maja / pass123
