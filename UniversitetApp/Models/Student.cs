using System.Text.RegularExpressions;

namespace UniversitetApp.Models;

/// <summary>
/// Representer en student i systemet.
/// Inneholder studentinformasjon og kurspåmeldinger.
/// StudentID valideres mot formatet S###.
/// </summary>
public class Student : Person
{
    /// <summary>Unik identifikator for studenten (format: S###)</summary>
    public string StudentID { get; private set; }
    
    private readonly List<string> _kursKoder = new();
    
    /// <summary>Skrivebeskyttet liste over alle kurs studenten er påmeldt</summary>
    public IReadOnlyList<string> KursKoder => _kursKoder;

    /// <summary>
    /// Initialiserer en ny student med validering av StudentID-format.
    /// </summary>
    /// <param name="studentID">Student-ID i format S### (f.eks. S001)</param>
    /// <param name="navn">Studentens navn</param>
    /// <param name="epost">Studentens e-postadresse</param>
    /// <exception cref="ArgumentException">Hvis StudentID ikke har korrekt format</exception>
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

    /// <summary>Returnerer formatert informasjon om studenten for visning</summary>
    /// <returns>Tekststreng med student-detaljer</returns>
    public override string HentInfo()
    {
        return $"[Student] {StudentID} | {Navn} | {Epost} | Påmeldt {_kursKoder.Count} kurs";
    }

    /// <summary>
    /// Melder studenten på et kurs.
    /// Hindrer tomme og dupliserte kursreferanser.
    /// </summary>
    /// <param name="kursKode">Kurskoden som skal legges til</param>
    /// <returns>true hvis kursen ble lagt til, false hvis invalid eller duplikat</returns>
    public bool LeggTilKurs(string kursKode)
    {
        if (string.IsNullOrWhiteSpace(kursKode)) return false;
        if (_kursKoder.Contains(kursKode, StringComparer.OrdinalIgnoreCase)) return false;
        _kursKoder.Add(kursKode);
        return true;
    }

    /// <summary>
    /// Melder studenten av et kurs.
    /// </summary>
    /// <param name="kursKode">Kurskoden som skal fjernes</param>
    /// <returns>true hvis kursen ble fjernet, false hvis ikke funnet</returns>
    public bool FjernKurs(string kursKode)
    {
        return _kursKoder.RemoveAll(k => k.Equals(kursKode, StringComparison.OrdinalIgnoreCase)) > 0;
    }
}
