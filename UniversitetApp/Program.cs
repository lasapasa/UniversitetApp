using UniversitetApp.Models;
using UniversitetApp.Services;

// ─── Oppstart ───────────────────────────────────────────────────────────────

var kursManager = new KursManager();
var bibliotekManager = new BibliotekManager();

// Eksempeldata så du har noe å jobbe med med en gang
SeedData(kursManager, bibliotekManager, out var studenter, out var ansatte);

// ─── Hovedmeny ──────────────────────────────────────────────────────────────

bool kjør = true;
while (kjør)
{
    Console.WriteLine("\n╔══════════════════════════════╗");
    Console.WriteLine("║    Universitetsystem v1.0    ║");
    Console.WriteLine("╠══════════════════════════════╣");
    Console.WriteLine("║  [1] Opprett kurs            ║");
    Console.WriteLine("║  [2] Meld student til kurs   ║");
    Console.WriteLine("║  [3] Print kurs og deltakere ║");
    Console.WriteLine("║  [4] Søk på kurs             ║");
    Console.WriteLine("║  [5] Søk på bok              ║");
    Console.WriteLine("║  [6] Lån bok                 ║");
    Console.WriteLine("║  [7] Returner bok            ║");
    Console.WriteLine("║  [8] Registrer bok           ║");
    Console.WriteLine("║  [0] Avslutt                 ║");
    Console.WriteLine("╚══════════════════════════════╝");
    Console.Write("Velg: ");

    string? valg = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (valg)
    {
        case "1":
            OpprettKurs(kursManager);
            break;
        case "2":
            MeldStudentTilKurs(kursManager, studenter);
            break;
        case "3":
            kursManager.PrintKursOgDeltakere(studenter);
            break;
        case "4":
            Console.Write("Søk (kurskode eller navn): ");
            kursManager.SøkEtterKurs(Console.ReadLine() ?? "");
            break;
        case "5":
            Console.Write("Søk (tittel, forfatter eller ID): ");
            bibliotekManager.SøkEtterBok(Console.ReadLine() ?? "");
            break;
        case "6":
            LånBok(bibliotekManager, studenter, ansatte);
            break;
        case "7":
            ReturnerBok(bibliotekManager, studenter, ansatte);
            break;
        case "8":
            RegistrerBok(bibliotekManager);
            break;
        case "0":
            Console.WriteLine("Ha det bra!");
            kjør = false;
            break;
        default:
            Console.WriteLine("Ugyldig valg. Prøv igjen.");
            break;
    }
}

// ─── Menymetoder ────────────────────────────────────────────────────────────

static void OpprettKurs(KursManager km)
{
    string kode = LesIkkeTom("Kurskode: ");
    string navn = LesIkkeTom("Kursnavn: ");
    int stp = LesInt("Studiepoeng: ");
    int maks = LesInt("Maks plasser: ");

    km.OpprettKurs(kode, navn, stp, maks);
}

static void MeldStudentTilKurs(KursManager km, List<Student> studenter)
{
    VisStudenter(studenter);
    string id = LesIkkeTom("StudentID: ");
    var student = studenter.FirstOrDefault(s => s.StudentID == id);
    if (student == null) { Console.WriteLine("Fant ikke studenten."); return; }

    string kode = LesIkkeTom("Kurskode: ");
    km.MeldPåKurs(student, kode);
}

static void LånBok(BibliotekManager bm, List<Student> studenter, List<Ansatt> ansatte)
{
    var bruker = VelgBruker(studenter, ansatte);
    if (bruker == null) return;

    string bokId = LesIkkeTom("Bok-ID: ");
    bm.LånUtBok(bruker, bokId);
}

static void ReturnerBok(BibliotekManager bm, List<Student> studenter, List<Ansatt> ansatte)
{
    var bruker = VelgBruker(studenter, ansatte);
    if (bruker == null) return;

    string bokId = LesIkkeTom("Bok-ID: ");
    bm.ReturnerBok(bruker, bokId);
}

static void RegistrerBok(BibliotekManager bm)
{
    Console.Write("Media-ID: ");
    string id = Console.ReadLine() ?? "";
    Console.Write("Tittel: ");
    string tittel = Console.ReadLine() ?? "";
    Console.Write("Forfatter: ");
    string forfatter = Console.ReadLine() ?? "";
    Console.Write("Utgivelsesår: ");
    int.TryParse(Console.ReadLine(), out int år);
    Console.Write("Antall eksemplarer: ");
    int.TryParse(Console.ReadLine(), out int antall);

    bm.RegistrerBok(id, tittel, forfatter, år, antall);
}

// ─── Hjelpemetoder ──────────────────────────────────────────────────────────

static IUser? VelgBruker(List<Student> studenter, List<Ansatt> ansatte)
{
    while (true)
    {
        Console.WriteLine("[1] Student  [2] Ansatt");
        string type = LesIkkeTom("Velg brukertype: ");

        if (type == "1")
        {
            VisStudenter(studenter);
            string id = LesIkkeTom("StudentID: ");
            var student = studenter.FirstOrDefault(s => s.StudentID == id);
            if (student == null)
            {
                Console.WriteLine("Fant ikke studenten.");
                continue;
            }

            return student;
        }

        if (type == "2")
        {
            Console.WriteLine("Ansatte:");
            foreach (var a in ansatte) Console.WriteLine($"  {a.AnsattID} – {a.Navn}");
            string id = LesIkkeTom("AnsattID: ");
            var ansatt = ansatte.FirstOrDefault(a => a.AnsattID == id);
            if (ansatt == null)
            {
                Console.WriteLine("Fant ikke ansatt.");
                continue;
            }

            return ansatt;
        }

        Console.WriteLine("Ugyldig valg.");
    }
}

static void VisStudenter(List<Student> studenter)
{
    Console.WriteLine("Studenter:");
    foreach (var s in studenter)
    Console.WriteLine($"  {s.HentInfo()}");
}

// ─── Seed-data ───────────────────────────────────────────────────────────────

static void SeedData(
    KursManager km,
    BibliotekManager bm,
    out List<Student> studenter,
    out List<Ansatt> ansatte)
{
    // Studenter
    studenter = new List<Student>
    {
        new Student("S001", "Kari Nordmann", "kari@uni.no"),
        new Student("S002", "Ola Hansen", "ola@uni.no"),
        new Student("S004", "Lina Aas", "lina@uni.no"),
        new Student("S005", "Jonas Berg", "jonas@uni.no"),
        new Utvekslingsstudent(
            "S003", "Emma Schmidt", "emma@berlin.de",
            "Humboldt-Universität", "Tyskland",
            new DateOnly(2025, 8, 1), new DateOnly(2026, 6, 30)),
        new Utvekslingsstudent(
            "S006", "Lucas Martin", "lucas@sorbonne.fr",
            "Sorbonne Université", "Frankrike",
            new DateOnly(2026, 1, 10), new DateOnly(2026, 12, 15))
    };

    // Ansatte
    ansatte = new List<Ansatt>
    {
        new Ansatt("A001", "Prof. Larsen", "larsen@uni.no", StillingType.Foreleser, "Informatikk"),
        new Ansatt("A002", "Maja Bakke", "maja@uni.no", StillingType.Bibliotekar, "Bibliotek"),
        new Ansatt("A003", "Eirik Nilsen", "eirik@uni.no", StillingType.Assistent, "Informatikk"),
        new Ansatt("A004", "Siri Holm", "siri@uni.no", StillingType.Administrator, "Studieadministrasjon")
    };

    // Kurs
    km.OpprettKurs("INF101", "Programmering 1", 10, 30);
    km.OpprettKurs("MAT201", "Lineær Algebra", 10, 25);
    km.OpprettKurs("INF201", "Objektorientert programmering", 10, 20);
    km.OpprettKurs("INF301", "Databaser", 10, 15);
    km.OpprettKurs("STA101", "Statistikk", 5, 40);

    // Meld på noen studenter
    km.MeldPåKurs(studenter[0], "INF101");
    km.MeldPåKurs(studenter[1], "INF101");
    km.MeldPåKurs(studenter[2], "MAT201");
    km.MeldPåKurs(studenter[3], "INF201");
    km.MeldPåKurs(studenter[4], "INF201");
    km.MeldPåKurs(studenter[5], "STA101");
    km.MeldPåKurs(studenter[0], "INF301");

    // Bøker
    bm.RegistrerBok("B001", "Clean Code", "Robert C. Martin", 2008, 3);
    bm.RegistrerBok("B002", "The Pragmatic Programmer", "Hunt & Thomas", 1999, 2);
    bm.RegistrerBok("B003", "C# in Depth", "Jon Skeet", 2019, 4);
    bm.RegistrerBok("B004", "Design Patterns", "Gamma et al.", 1994, 2);
    bm.RegistrerBok("B005", "Introduction to Algorithms", "Cormen et al.", 2009, 1);

    // Lån (blanding av aktive og returnerte)
    bm.LånUtBok(studenter[0], "B001");
    bm.LånUtBok(studenter[1], "B002");
    bm.LånUtBok(ansatte[0], "B003");
    bm.LånUtBok(studenter[2], "B004");
    bm.ReturnerBok(studenter[2], "B004");
    bm.LånUtBok(ansatte[1], "B005");

    Console.WriteLine("\n[Eksempeldata lastet inn]");
}

static string LesIkkeTom(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string verdi = (Console.ReadLine() ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(verdi)) return verdi;
        Console.WriteLine("Feil: Input kan ikke være tom.");
    }
}

static int LesInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse((Console.ReadLine() ?? string.Empty).Trim(), out int verdi))
            return verdi;
        Console.WriteLine("Feil: Skriv inn et gyldig heltall.");
    }
}
