using UniversitetApp.Models;

namespace UniversitetApp.Services;

// Håndterer all kursrelatert forretningslogikk: opprettelse, påmelding og søk.
public class KursManager
{
    private readonly List<Kurs> _kurs = new();

    public IReadOnlyList<Kurs> HentAlleKurs() => _kurs;

    public List<Kurs> FinnKursEtterSøk(string søkeord)
    {
        if (string.IsNullOrWhiteSpace(søkeord)) return _kurs.ToList();

        return _kurs.Where(k =>
            k.KursKode.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            k.Navn.Contains(søkeord, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public OperationResult OpprettKurs(string kode, string navn, int studiepoeng, int maksPlasser, string lærerAnsattID)
    {
        if (string.IsNullOrWhiteSpace(kode) || string.IsNullOrWhiteSpace(navn))
        {
            return OperationResult.Failure("Kurskode og kursnavn må fylles ut.", "validation_error");
        }

        if (studiepoeng <= 0 || maksPlasser <= 0)
        {
            return OperationResult.Failure("Studiepoeng og maks plasser må være større enn 0.", "validation_error");
        }

        if (_kurs.Any(k => k.KursKode.Equals(kode, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Failure($"Kurs med kode '{kode}' finnes allerede.", "duplicate_course_code");
        }

        if (_kurs.Any(k => k.Navn.Equals(navn, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Failure($"Kurs med navn '{navn}' finnes allerede.", "duplicate_course_name");
        }

        try
        {
            _kurs.Add(new Kurs(kode, navn, studiepoeng, maksPlasser, lærerAnsattID));
            return OperationResult.Success($"Kurs opprettet: {kode} - {navn}");
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Failure(ex.Message, "validation_error");
        }
    }

    public OperationResult MeldPåKurs(Student student, string kursKode)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
        {
            return OperationResult.Failure("Kurskode kan ikke være tom.", "validation_error");
        }

        var kurs = FinnKurs(kursKode, out var finnFeil);
        if (kurs == null)
        {
            return OperationResult.Failure(finnFeil, "course_not_found");
        }

        if (kurs.ErFull)
        {
            return OperationResult.Failure($"Kurset '{kurs.Navn}' er fullt ({kurs.MaksPlasser} plasser).", "course_full");
        }

        if (kurs.DeltakerIDs.Contains(student.StudentID, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Failure($"{student.Navn} er allerede påmeldt '{kurs.Navn}'.", "already_enrolled");
        }

        kurs.LeggTilDeltaker(student.StudentID);
        student.LeggTilKurs(kurs.KursKode);
        return OperationResult.Success($"{student.Navn} er nå påmeldt '{kurs.Navn}'. Ledige plasser: {kurs.LedigePlasser}");
    }

    public OperationResult MeldAvKurs(Student student, string kursKode)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
        {
            return OperationResult.Failure("Kurskode kan ikke være tom.", "validation_error");
        }

        var kurs = FinnKurs(kursKode, out var finnFeil);
        if (kurs == null)
        {
            return OperationResult.Failure(finnFeil, "course_not_found");
        }

        bool fjernet = kurs.FjernDeltaker(student.StudentID);
        student.FjernKurs(kursKode);

        if (fjernet)
        {
            return OperationResult.Success($"{student.Navn} er meldt av '{kurs.Navn}'.");
        }
        else
        {
            return OperationResult.Failure($"{student.Navn} er ikke påmeldt '{kurs.Navn}'.", "not_enrolled");
        }
    }

    public OperationResult SettKarakter(Ansatt lærer, string kursKode, string studentID, string karakter)
    {
        if (lærer.Stilling != StillingType.Foreleser)
        {
            return OperationResult.Failure("Kun faglærer (Foreleser) kan sette karakter.", "insufficient_role");
        }

        var kurs = FinnKurs(kursKode, out var finnFeil);
        if (kurs == null)
        {
            return OperationResult.Failure(finnFeil, "course_not_found");
        }

        if (!kurs.LærerAnsattID.Equals(lærer.AnsattID, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Failure("Du kan kun sette karakter i kurs du underviser.", "not_course_teacher");
        }

        if (!kurs.DeltakerIDs.Contains(studentID, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Failure("Studenten er ikke meldt på kurset.", "student_not_enrolled");
        }

        if (!ErGyldigKarakter(karakter))
        {
            return OperationResult.Failure("Ugyldig karakter. Gyldige verdier: A, B, C, D, E, F.", "invalid_grade");
        }

        kurs.SettKarakter(studentID, karakter);
        return OperationResult.Success("Karakter registrert.");
    }

    public OperationResult RegistrerPensum(Ansatt lærer, string kursKode, string bokID)
    {
        if (lærer.Stilling != StillingType.Foreleser)
        {
            return OperationResult.Failure("Kun faglærer (Foreleser) kan registrere pensum.", "insufficient_role");
        }

        var kurs = FinnKurs(kursKode, out var finnFeil);
        if (kurs == null)
        {
            return OperationResult.Failure(finnFeil, "course_not_found");
        }

        if (!kurs.LærerAnsattID.Equals(lærer.AnsattID, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Failure("Du kan kun registrere pensum i kurs du underviser.", "not_course_teacher");
        }

        if (!kurs.LeggTilPensum(bokID))
        {
            return OperationResult.Failure("Pensumbok er ugyldig eller finnes allerede i kurset.", "invalid_or_duplicate_pensum");
        }

        return OperationResult.Success("Pensum registrert.");
    }

    public List<Kurs> HentKursForStudent(string studentID)
    {
        return _kurs.Where(k => k.DeltakerIDs.Contains(studentID, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public List<Kurs> HentKursForLærer(string ansattID)
    {
        return _kurs.Where(k => k.LærerAnsattID.Equals(ansattID, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public string HentKarakter(Student student, string kursKode)
    {
        var kurs = _kurs.FirstOrDefault(k => k.KursKode.Equals(kursKode, StringComparison.OrdinalIgnoreCase));
        if (kurs == null) return "Kurs ikke funnet.";

        if (!kurs.DeltakerIDs.Contains(student.StudentID, StringComparer.OrdinalIgnoreCase))
            return "Ikke påmeldt kurs.";

        return kurs.TryGetKarakter(student.StudentID, out var karakter)
            ? karakter ?? "-"
            : "Ikke satt";
    }

    private Kurs? FinnKurs(string kursKode, out string feil)
    {
        feil = string.Empty;
        var kurs = _kurs.FirstOrDefault(k => k.KursKode.Equals(kursKode, StringComparison.OrdinalIgnoreCase));
        if (kurs == null)
            feil = $"Fant ikke kurs med kode '{kursKode}'.";
        return kurs;
    }

    private static bool ErGyldigKarakter(string karakter)
    {
        string[] gyldige = ["A", "B", "C", "D", "E", "F"];
        return gyldige.Contains((karakter ?? string.Empty).Trim().ToUpperInvariant());
    }

}
