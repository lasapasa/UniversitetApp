namespace UniversitetApp.Models;

// Kurs holder fagdata og en kapslet deltakerliste med student-ID-er.
public class Kurs
{
    public string KursKode { get; set; }
    public string Navn { get; set; }
    public int Studiepoeng { get; set; }
    public int MaksPlasser { get; set; }
    private readonly List<string> _deltakerIDs = new();
    public IReadOnlyList<string> DeltakerIDs => _deltakerIDs;

    public int LedigePlasser => MaksPlasser - _deltakerIDs.Count;
    public bool ErFull => _deltakerIDs.Count >= MaksPlasser;

    public Kurs(string kursKode, string navn, int studiepoeng, int maksPlasser)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
            throw new ArgumentException("Kurskode kan ikke være tom.", nameof(kursKode));
        if (string.IsNullOrWhiteSpace(navn))
            throw new ArgumentException("Kursnavn kan ikke være tom.", nameof(navn));
        if (studiepoeng <= 0)
            throw new ArgumentException("Studiepoeng må være større enn 0.", nameof(studiepoeng));
        if (maksPlasser <= 0)
            throw new ArgumentException("Maks plasser må være større enn 0.", nameof(maksPlasser));

        KursKode = kursKode;
        Navn = navn;
        Studiepoeng = studiepoeng;
        MaksPlasser = maksPlasser;
    }

    public override string ToString()
    {
        return $"{KursKode} | {Navn} | {Studiepoeng} stp | {_deltakerIDs.Count}/{MaksPlasser} plasser";
    }

    public bool LeggTilDeltaker(string studentID)
    {
        // Sikrer at samme student ikke registreres flere ganger.
        if (string.IsNullOrWhiteSpace(studentID)) return false;
        if (_deltakerIDs.Contains(studentID, StringComparer.OrdinalIgnoreCase)) return false;
        _deltakerIDs.Add(studentID);
        return true;
    }

    public bool FjernDeltaker(string studentID)
    {
        return _deltakerIDs.RemoveAll(id => id.Equals(studentID, StringComparison.OrdinalIgnoreCase)) > 0;
    }
}
