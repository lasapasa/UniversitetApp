using System.Text.Json.Serialization;
using UniversitetApp.Models;

namespace UniversitetApp.Services;

/// <summary>
/// Mapper mellom persistert tilstand (JSON) og in-memory objekter.
/// Konverterer mellom DTO-er og domeneobjekter begge veier.
/// </summary>
internal static class AppStateMapper
{
    /// <summary>
    /// Fyller snapshot med data fra persistert tilstand.
    /// Konverterer DTOer til domeneobjekter og oppbygger alle relasjoner.
    /// </summary>
    /// <param name="persisted">Persistert tilstand fra JSON-fil</param>
    /// <returns>Snapshot med alle domeneobjekter fylt med data</returns>
    public static AppStateSnapshot FyllSnapshot(PersistedAppState persisted)
    {
        var studenter = FyllStudenter(persisted.Studenter);
        var ansatte = FyllAnsatte(persisted.Ansatte);
        var kurs = FyllKurs(persisted.Kurs, ansatte);
        var boker = FyllBoker(persisted.Boker);
        var laan = FyllLaan(persisted.Laan, studenter, ansatte, boker);
        var accounts = FyllAccounts(persisted.Accounts);
        if (accounts.Count == 0)
        {
            accounts = GenererStandardKontoer(studenter, ansatte);
        }

        SynkStudentKursFraKursliste(studenter, kurs);

        return new AppStateSnapshot(studenter, ansatte, kurs, boker, laan, accounts);
    }

    public static PersistedAppState ByggPersistedState(AppStateSnapshot snapshot)
    {
        return new PersistedAppState
        {
            Studenter = snapshot.Studenter.Select(s =>
            {
                var dto = new StudentDto
                {
                    Type = s is Utvekslingsstudent ? "Utvekslingsstudent" : "Student",
                    StudentID = s.StudentID,
                    Navn = s.Navn,
                    Epost = s.Epost,
                    Kurs = s.KursKoder.ToList()
                };

                if (s is Utvekslingsstudent us)
                {
                    dto.Hjemuniversitet = us.Hjemuniversitet;
                    dto.Land = us.Land;
                    dto.PeriodeFra = us.PeriodeFra;
                    dto.PeriodeTil = us.PeriodeTil;
                }

                return dto;
            }).ToList(),
            Ansatte = snapshot.Ansatte.Select(a => new AnsattDto
            {
                AnsattID = a.AnsattID,
                Navn = a.Navn,
                Epost = a.Epost,
                Stilling = a.Stilling.ToString(),
                Avdeling = a.Avdeling
            }).ToList(),
            Kurs = snapshot.Kurs.Select(k => new KursDto
            {
                KursKode = k.KursKode,
                Navn = k.Navn,
                Studiepoeng = k.Studiepoeng,
                MaksPlasser = k.MaksPlasser,
                LaererAnsattID = k.LærerAnsattID,
                Deltakere = k.DeltakerIDs.ToList(),
                PensumBokIDs = k.PensumBokIDs.ToList(),
                Karakterer = k.Karakterer.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase)
            }).ToList(),
            Boker = snapshot.Boker.Select(b => new BokDto
            {
                MediaID = b.MediaID,
                Tittel = b.Tittel,
                Forfatter = b.Forfatter,
                Ar = b.År,
                AntallEksemplarer = b.AntallEksemplarer,
                TilgjengeligeEksemplarer = b.TilgjengeligeEksemplarer
            }).ToList(),
            Laan = snapshot.LaanHistorikk.Select(l =>
            {
                string brukerType = l.Bruker is Student ? "Student" : "Ansatt";
                string brukerId = l.Bruker is Student s ? s.StudentID : ((Ansatt)l.Bruker).AnsattID;

                return new LaanDto
                {
                    BrukerType = brukerType,
                    BrukerID = brukerId,
                    MediaID = l.Bok.MediaID,
                    LaanDato = l.LånDato,
                    ReturDato = l.ReturDato
                };
            }).ToList(),
            Accounts = snapshot.Accounts.Select(a => new AccountDto
            {
                Username = a.Username,
                Password = a.Password,
                Role = a.Role.ToString(),
                ReferenceID = a.ReferenceId
            }).ToList()
        };
    }

    /// <summary>
    /// Fyller studentliste med data fra DTO-er.
    /// Håndterer både vanlige studenter og utvekslingsstudenter.
    /// </summary>
    private static List<Student> FyllStudenter(List<StudentDto> dtos)
    {
        var studenter = new List<Student>();

        foreach (var dto in dtos)
        {
            Student student;
            bool erUtveksling = dto.Type.Equals("Utvekslingsstudent", StringComparison.OrdinalIgnoreCase);
            if (erUtveksling &&
                !string.IsNullOrWhiteSpace(dto.Hjemuniversitet) &&
                !string.IsNullOrWhiteSpace(dto.Land) &&
                dto.PeriodeFra.HasValue &&
                dto.PeriodeTil.HasValue)
            {
                student = new Utvekslingsstudent(
                    dto.StudentID,
                    dto.Navn,
                    dto.Epost,
                    dto.Hjemuniversitet,
                    dto.Land,
                    dto.PeriodeFra.Value,
                    dto.PeriodeTil.Value);
            }
            else
            {
                student = new Student(dto.StudentID, dto.Navn, dto.Epost);
            }

            foreach (var kursKode in dto.Kurs)
            {
                student.LeggTilKurs(kursKode);
            }

            studenter.Add(student);
        }

        return studenter;
    }

    /// <summary>
    /// Fyller ansattliste med data fra DTO-er.
    /// Mapper stillingtype til korrekt enum-verdi.
    /// </summary>
    private static List<Ansatt> FyllAnsatte(List<AnsattDto> dtos)
    {
        var ansatte = new List<Ansatt>();

        foreach (var dto in dtos)
        {
            if (!Enum.TryParse<StillingType>(dto.Stilling, true, out var stilling))
            {
                continue;
            }

            ansatte.Add(new Ansatt(dto.AnsattID, dto.Navn, dto.Epost, stilling, dto.Avdeling));
        }

        return ansatte;
    }

    /// <summary>
    /// Fyller kursliste med data fra DTO-er.
    /// Bygger opp deltakere, pensum, og karakterer for hvert kurs.
    /// Bruker fallback-lærer hvis ingen lærer er spesifisert.
    /// </summary>
    private static List<Kurs> FyllKurs(List<KursDto> dtos, List<Ansatt> ansatte)
    {
        var kursListe = new List<Kurs>();
        string fallbackLaerer = ansatte.FirstOrDefault(a => a.Stilling == StillingType.Foreleser)?.AnsattID ?? "A001";

        foreach (var dto in dtos)
        {
            string laererAnsattId = string.IsNullOrWhiteSpace(dto.LaererAnsattID) ? fallbackLaerer : dto.LaererAnsattID;
            var kurs = new Kurs(dto.KursKode, dto.Navn, dto.Studiepoeng, dto.MaksPlasser, laererAnsattId);

            foreach (var studentId in dto.Deltakere)
            {
                kurs.LeggTilDeltaker(studentId);
            }

            foreach (var bokId in dto.PensumBokIDs)
            {
                kurs.LeggTilPensum(bokId);
            }

            foreach (var entry in dto.Karakterer)
            {
                kurs.SettKarakter(entry.Key, entry.Value);
            }

            kursListe.Add(kurs);
        }

        return kursListe;
    }

    /// <summary>
    /// Fyller bøker-liste med data fra DTO-er.
    /// Sikrer at tilgjengelige eksemplarer ikke overstiger totalt antall.
    /// </summary>
    private static List<Bok> FyllBoker(List<BokDto> dtos)
    {
        var boker = new List<Bok>();

        foreach (var dto in dtos)
        {
            var bok = new Bok(dto.MediaID, dto.Tittel, dto.Forfatter, dto.Ar, dto.AntallEksemplarer);
            bok.TilgjengeligeEksemplarer = Math.Clamp(dto.TilgjengeligeEksemplarer, 0, dto.AntallEksemplarer);
            boker.Add(bok);
        }

        return boker;
    }

    /// <summary>
    /// Fyller lånhistorikk med data fra DTO-er.
    /// Bygger indekser for rask oppslag av studenter, ansatte og bøker.
    /// Skipper låner hvis bruker eller bok ikke finnes.
    /// </summary>
    private static List<Laan> FyllLaan(List<LaanDto> dtos, List<Student> studenter, List<Ansatt> ansatte, List<Bok> boker)
    {
        var laanHistorikk = new List<Laan>();
        var studenterById = ByggStudentIndeks(studenter);
        var ansatteById = ByggAnsattIndeks(ansatte);
        var bokerById = ByggBokIndeks(boker);

        foreach (var dto in dtos)
        {
            if (!bokerById.TryGetValue(dto.MediaID, out var bok))
            {
                continue;
            }

            IUser? bruker = null;
            if (dto.BrukerType.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                studenterById.TryGetValue(dto.BrukerID, out var student);
                bruker = student;
            }
            else if (dto.BrukerType.Equals("Ansatt", StringComparison.OrdinalIgnoreCase))
            {
                ansatteById.TryGetValue(dto.BrukerID, out var ansatt);
                bruker = ansatt;
            }

            if (bruker == null)
            {
                continue;
            }

            var laan = new Laan(bruker, bok)
            {
                LånDato = dto.LaanDato,
                ReturDato = dto.ReturDato
            };

            laanHistorikk.Add(laan);
        }

        return laanHistorikk;
    }

    /// <summary>
    /// Fyller konto-liste med data fra DTO-er.
    /// Mapper rolle string til AppRole enum.
    /// </summary>
    private static List<UserAccount> FyllAccounts(List<AccountDto> dtos)
    {
        var accounts = new List<UserAccount>();

        foreach (var dto in dtos)
        {
            if (!Enum.TryParse<AppRole>(dto.Role, true, out var role))
            {
                continue;
            }

            accounts.Add(new UserAccount(dto.Username, dto.Password, role, dto.ReferenceID));
        }

        return accounts;
    }

    /// <summary>
    /// Genererer standardkontoer for alle studenter og ansatte hvis ingen kontoer finnes.
    /// Lager unike brukernavn basert på e-post eller ID.
    /// </summary>
    private static List<UserAccount> GenererStandardKontoer(List<Student> studenter, List<Ansatt> ansatte)
    {
        var kontoer = new List<UserAccount>();
        var brukernavn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var student in studenter)
        {
            string username = FinnUniktBrukernavn(student.Epost, student.StudentID, brukernavn);
            kontoer.Add(new UserAccount(username, "pass123", AppRole.Student, student.StudentID));
        }

        foreach (var ansatt in ansatte)
        {
            AppRole role = ansatt.Stilling == StillingType.Foreleser
                ? AppRole.Faglærer
                : AppRole.BibliotekAnsatt;

            string username = FinnUniktBrukernavn(ansatt.Epost, ansatt.AnsattID, brukernavn);
            kontoer.Add(new UserAccount(username, "pass123", role, ansatt.AnsattID));
        }

        return kontoer;
    }

    /// <summary>
    /// Finner unikt brukernavn ved å legge til suffix hvis nødvendig.
    /// Erstatter eksisterende brukernavn i HashSet-en.
    /// </summary>
    private static string FinnUniktBrukernavn(string epost, string fallbackId, HashSet<string> brukernavn)
    {
        string kandidat = epost.Contains('@') ? epost.Split('@')[0] : fallbackId;
        kandidat = string.IsNullOrWhiteSpace(kandidat) ? fallbackId : kandidat.Trim().ToLowerInvariant();

        string unik = kandidat;
        int suffix = 1;
        while (!brukernavn.Add(unik))
        {
            unik = $"{kandidat}{suffix}";
            suffix++;
        }

        return unik;
    }

    /// <summary>
    /// Synkroniserer studentenes kursliste med kursenes deltakerliste.
    /// Sikrer at alle relasjoner er oppdatert etter innlesing av data.
    /// </summary>
    private static void SynkStudentKursFraKursliste(List<Student> studenter, List<Kurs> kursListe)
    {
        var studenterById = ByggStudentIndeks(studenter);

        foreach (var kurs in kursListe)
        {
            foreach (var studentId in kurs.DeltakerIDs)
            {
                if (studenterById.TryGetValue(studentId, out var student))
                {
                    student.LeggTilKurs(kurs.KursKode);
                }
            }
        }
    }

    /// <summary>
    /// Bygger indeks (dictionary) over studenter for rask oppslag etter StudentID.
    /// Case-insensitive nøkler for sikkerhet.
    /// </summary>
    private static Dictionary<string, Student> ByggStudentIndeks(IEnumerable<Student> studenter)
    {
        var indeks = new Dictionary<string, Student>(StringComparer.OrdinalIgnoreCase);
        foreach (var student in studenter)
        {
            if (!indeks.ContainsKey(student.StudentID))
            {
                indeks[student.StudentID] = student;
            }
        }

        return indeks;
    }

    /// <summary>
    /// Bygger indeks over ansatte for rask oppslag etter AnsattID.
    /// </summary>
    private static Dictionary<string, Ansatt> ByggAnsattIndeks(IEnumerable<Ansatt> ansatte)
    {
        var indeks = new Dictionary<string, Ansatt>(StringComparer.OrdinalIgnoreCase);
        foreach (var ansatt in ansatte)
        {
            if (!indeks.ContainsKey(ansatt.AnsattID))
            {
                indeks[ansatt.AnsattID] = ansatt;
            }
        }

        return indeks;
    }

    /// <summary>
    /// Bygger indeks over bøker for rask oppslag etter MediaID.
    /// </summary>
    private static Dictionary<string, Bok> ByggBokIndeks(IEnumerable<Bok> boker)
    {
        var indeks = new Dictionary<string, Bok>(StringComparer.OrdinalIgnoreCase);
        foreach (var bok in boker)
        {
            if (!indeks.ContainsKey(bok.MediaID))
            {
                indeks[bok.MediaID] = bok;
            }
        }

        return indeks;
    }
}

internal sealed class PersistedAppState
{
    public List<StudentDto> Studenter { get; set; } = new();
    public List<AnsattDto> Ansatte { get; set; } = new();
    public List<KursDto> Kurs { get; set; } = new();

    [JsonPropertyName("Boker")]
    public List<BokDto> Boker { get; set; } = new();

    [JsonPropertyName("Bøker")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BokDto>? BoekerLegacy
    {
        get => null;
        set => Boker = value ?? new List<BokDto>();
    }

    [JsonPropertyName("Laan")]
    public List<LaanDto> Laan { get; set; } = new();

    [JsonPropertyName("Lån")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<LaanDto>? LaanLegacy
    {
        get => null;
        set => Laan = value ?? new List<LaanDto>();
    }

    public List<AccountDto> Accounts { get; set; } = new();
}

internal sealed class StudentDto
{
    public string Type { get; set; } = "Student";
    public string StudentID { get; set; } = string.Empty;
    public string Navn { get; set; } = string.Empty;
    public string Epost { get; set; } = string.Empty;
    public List<string> Kurs { get; set; } = new();
    public string? Hjemuniversitet { get; set; }
    public string? Land { get; set; }
    public DateOnly? PeriodeFra { get; set; }
    public DateOnly? PeriodeTil { get; set; }
}

internal sealed class AnsattDto
{
    public string AnsattID { get; set; } = string.Empty;
    public string Navn { get; set; } = string.Empty;
    public string Epost { get; set; } = string.Empty;
    public string Stilling { get; set; } = string.Empty;
    public string Avdeling { get; set; } = string.Empty;
}

internal sealed class KursDto
{
    public string KursKode { get; set; } = string.Empty;
    public string Navn { get; set; } = string.Empty;
    public int Studiepoeng { get; set; }
    public int MaksPlasser { get; set; }
    public string? LaererAnsattID { get; set; }
    public List<string> Deltakere { get; set; } = new();
    public List<string> PensumBokIDs { get; set; } = new();
    public Dictionary<string, string> Karakterer { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class BokDto
{
    public string MediaID { get; set; } = string.Empty;
    public string Tittel { get; set; } = string.Empty;
    public string Forfatter { get; set; } = string.Empty;

    [JsonPropertyName("Ar")]
    public int Ar { get; set; }

    [JsonPropertyName("År")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AarLegacy
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                Ar = value.Value;
            }
        }
    }

    public int AntallEksemplarer { get; set; }
    public int TilgjengeligeEksemplarer { get; set; }
}

internal sealed class LaanDto
{
    public string BrukerType { get; set; } = string.Empty;
    public string BrukerID { get; set; } = string.Empty;
    public string MediaID { get; set; } = string.Empty;
    public DateTime LaanDato { get; set; }
    public DateTime? ReturDato { get; set; }
}

internal sealed class AccountDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ReferenceID { get; set; } = string.Empty;
}
