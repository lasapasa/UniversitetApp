using UniversitetApp.Models;

namespace UniversitetApp.Services;

// Håndterer all kursrelatert forretningslogikk: opprettelse, påmelding og søk.
public class KursManager
{
    private List<Kurs> _kurs = new();

    public void OpprettKurs(string kode, string navn, int studiepoeng, int maksPlasser)
    {
        if (string.IsNullOrWhiteSpace(kode) || string.IsNullOrWhiteSpace(navn))
        {
            Console.WriteLine("Feil: Kurskode og kursnavn må fylles ut.");
            return;
        }

        if (studiepoeng <= 0 || maksPlasser <= 0)
        {
            Console.WriteLine("Feil: Studiepoeng og maks plasser må være større enn 0.");
            return;
        }

        if (_kurs.Any(k => k.KursKode == kode))
        {
            Console.WriteLine($"Feil: Kurs med kode '{kode}' finnes allerede.");
            return;
        }

        _kurs.Add(new Kurs(kode, navn, studiepoeng, maksPlasser));
        Console.WriteLine($"Kurs opprettet: {kode} – {navn}");
    }

    public void MeldPåKurs(Student student, string kursKode)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
        {
            Console.WriteLine("Feil: Kurskode kan ikke være tom.");
            return;
        }

        // Slår opp kurs én gang og stopper tidlig ved feil.
        var kurs = FinnKurs(kursKode);
        if (kurs == null) return;

        if (kurs.ErFull)
        {
            Console.WriteLine($"Feil: Kurset '{kurs.Navn}' er fullt ({kurs.MaksPlasser} plasser).");
            return;
        }

        if (kurs.DeltakerIDs.Contains(student.StudentID, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Feil: {student.Navn} er allerede påmeldt '{kurs.Navn}'.");
            return;
        }

        kurs.LeggTilDeltaker(student.StudentID);
        student.LeggTilKurs(kurs.KursKode);
        Console.WriteLine($"{student.Navn} er nå påmeldt '{kurs.Navn}'. Ledige plasser: {kurs.LedigePlasser}");
    }

    public void MeldAvKurs(Student student, string kursKode)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
        {
            Console.WriteLine("Feil: Kurskode kan ikke være tom.");
            return;
        }

        var kurs = FinnKurs(kursKode);
        if (kurs == null) return;

        bool fjernet = kurs.FjernDeltaker(student.StudentID);
        student.FjernKurs(kursKode);

        if (fjernet)
            Console.WriteLine($"{student.Navn} er meldt av '{kurs.Navn}'.");
        else
            Console.WriteLine($"Feil: {student.Navn} er ikke påmeldt '{kurs.Navn}'.");
    }

    public void PrintKursOgDeltakere(List<Student> studenter)
    {
        if (_kurs.Count == 0)
        {
            Console.WriteLine("Ingen kurs registrert.");
            return;
        }

        _kurs.ForEach(kurs =>
        {
            Console.WriteLine($"\n{kurs}");
            if (!kurs.DeltakerIDs.Any())
            {
                Console.WriteLine("  Ingen deltakere.");
            }
            else
            {
                // Maper ID-er til studentobjekter for lesbar utskrift.
                kurs.DeltakerIDs
                    .Select(id => studenter.FirstOrDefault(s => s.StudentID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    .Where(s => s != null)
                    .ToList()
                    .ForEach(s => Console.WriteLine($"  - {s!.Navn} ({s.StudentID})"));
            }
        });
    }

    public void SøkEtterKurs(string søkeord)
    {
        var treff = _kurs.Where(k =>
            k.KursKode.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            k.Navn.Contains(søkeord, StringComparison.OrdinalIgnoreCase)).ToList();

        if (treff.Count == 0)
        {
            Console.WriteLine($"Ingen kurs funnet for '{søkeord}'.");
            return;
        }

        Console.WriteLine($"Fant {treff.Count} kurs:");
        foreach (var kurs in treff)
            Console.WriteLine($"  {kurs}");
    }

    private Kurs? FinnKurs(string kursKode)
    {
        var kurs = _kurs.FirstOrDefault(k => k.KursKode.Equals(kursKode, StringComparison.OrdinalIgnoreCase));
        if (kurs == null)
            Console.WriteLine($"Feil: Fant ikke kurs med kode '{kursKode}'.");
        return kurs;
    }

}
