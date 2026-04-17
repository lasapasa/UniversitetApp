namespace UniversitetApp.Models;

/// <summary>
/// Representerer et kurs i systemet.
/// Inneholder fagdata, deltakerliste, pensum og karakterer.
/// </summary>
public class Kurs
{
    /// <summary>Unik kurskode (f.eks. INF101)</summary>
    public string KursKode { get; set; }
    
    /// <summary>Kursets navn</summary>
    public string Navn { get; set; }
    
    /// <summary>Antall studiepoeng</summary>
    public int Studiepoeng { get; set; }
    
    /// <summary>Maksimalt antall deltakere</summary>
    public int MaksPlasser { get; set; }
    
    /// <summary>Ansatt-ID for underviseren av kurset</summary>
    public string LærerAnsattID { get; private set; }
    
    private readonly List<string> _deltakerIDs = new();
    private readonly List<string> _pensumBokIDs = new();
    private readonly Dictionary<string, string> _karakterer = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>Skrivebeskyttet liste over student-ID-er som er påmeldt kurset</summary>
    public IReadOnlyList<string> DeltakerIDs => _deltakerIDs;
    
    /// <summary>Skrivebeskyttet liste over pensum-bok-ID-er</summary>
    public IReadOnlyList<string> PensumBokIDs => _pensumBokIDs;
    
    /// <summary>Skrivebeskyttet dictionary over studenters karakterer (StudentID → Karakter)</summary>
    public IReadOnlyDictionary<string, string> Karakterer => _karakterer;

    /// <summary>Beregnet: hvor mange plasser som er ledige</summary>
    public int LedigePlasser => MaksPlasser - _deltakerIDs.Count;
    
    /// <summary>Beregnet: om kurset er fullt oppsatt</summary>
    public bool ErFull => _deltakerIDs.Count >= MaksPlasser;

    public Kurs(string kursKode, string navn, int studiepoeng, int maksPlasser, string lærerAnsattID)
    {
        if (string.IsNullOrWhiteSpace(kursKode))
            throw new ArgumentException("Kurskode kan ikke være tom.", nameof(kursKode));
        if (string.IsNullOrWhiteSpace(navn))
            throw new ArgumentException("Kursnavn kan ikke være tom.", nameof(navn));
        if (studiepoeng <= 0)
            throw new ArgumentException("Studiepoeng må være større enn 0.", nameof(studiepoeng));
        if (maksPlasser <= 0)
            throw new ArgumentException("Maks plasser må være større enn 0.", nameof(maksPlasser));
        if (string.IsNullOrWhiteSpace(lærerAnsattID))
            throw new ArgumentException("LærerAnsattID kan ikke være tom.", nameof(lærerAnsattID));

        KursKode = kursKode.Trim().ToUpperInvariant();
        Navn = navn;
        Studiepoeng = studiepoeng;
        MaksPlasser = maksPlasser;
        LærerAnsattID = lærerAnsattID.Trim().ToUpperInvariant();
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

    public bool LeggTilPensum(string bokID)
    {
        if (string.IsNullOrWhiteSpace(bokID)) return false;

        string normalisertBokId = bokID.Trim().ToUpperInvariant();
        if (_pensumBokIDs.Contains(normalisertBokId, StringComparer.OrdinalIgnoreCase)) return false;
        _pensumBokIDs.Add(normalisertBokId);
        return true;
    }

    public bool SettKarakter(string studentID, string karakter)
    {
        if (string.IsNullOrWhiteSpace(studentID) || string.IsNullOrWhiteSpace(karakter)) return false;
        if (!_deltakerIDs.Contains(studentID, StringComparer.OrdinalIgnoreCase)) return false;

        _karakterer[studentID] = karakter.Trim().ToUpperInvariant();
        return true;
    }

    public bool TryGetKarakter(string studentID, out string? karakter)
    {
        return _karakterer.TryGetValue(studentID, out karakter);
    }
}
