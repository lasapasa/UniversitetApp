using System.Text.RegularExpressions;

namespace UniversitetApp.Models;

// Domeneobjekt for student med kapslet liste over kurskoder.
public class Student : Person
{
    public string StudentID { get; private set; }
    private readonly List<string> _kursKoder = new();
    public IReadOnlyList<string> KursKoder => _kursKoder;

    public Student(string studentID, string navn, string epost)
        : base(navn, epost)
    {
        if (string.IsNullOrWhiteSpace(studentID))
            throw new ArgumentException("StudentID kan ikke være tom.", nameof(studentID));

        string normalisertId = studentID.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(normalisertId, "^S\\d{3}$"))
            throw new ArgumentException("StudentID må ha formatet S### (f.eks. S001).", nameof(studentID));

        StudentID = normalisertId;
    }

    // override av abstrakt metode fra Person – polymorfisme
    public override string HentInfo()
    {
        return $"[Student] {StudentID} | {Navn} | {Epost} | Påmeldt {_kursKoder.Count} kurs";
    }

    public bool LeggTilKurs(string kursKode)
    {
        // Hindrer tomme og dupliserte kursreferanser.
        if (string.IsNullOrWhiteSpace(kursKode)) return false;
        if (_kursKoder.Contains(kursKode, StringComparer.OrdinalIgnoreCase)) return false;
        _kursKoder.Add(kursKode);
        return true;
    }

    public bool FjernKurs(string kursKode)
    {
        return _kursKoder.RemoveAll(k => k.Equals(kursKode, StringComparison.OrdinalIgnoreCase)) > 0;
    }
}
