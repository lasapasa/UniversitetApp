using System.Text.Json.Serialization;

namespace UniversitetApp.Models;

/// <summary>
/// Spesialisert studenttype som representerer en student på utveksling.
/// Inneholder ekstra informasjon om hjemuniversitet og oppholdsperiode.
/// </summary>
public class Utvekslingsstudent : Student
{
    /// <summary>Navn på hjemuniversitet</summary>
    public string Hjemuniversitet { get; set; }
    
    /// <summary>Land hvor hjemuniversitetet er lokalisert</summary>
    public string Land { get; set; }
    
    /// <summary>Startdato for utvekslingsstudiet</summary>
    public DateOnly PeriodeFra { get; set; }
    
    /// <summary>Sluttdato for utvekslingsstudiet</summary>
    public DateOnly PeriodeTil { get; set; }

    /// <summary>
    /// Initialiserer en ny utvekslingsstudent.
    /// Brukes ved JSON-deserialisering.
    /// </summary>
    /// <param name="studentID">Student-ID i format S###</param>
    /// <param name="navn">Studentens navn</param>
    /// <param name="epost">Studentens e-postadresse</param>
    /// <param name="hjemuniversitet">Navn på hjemuniversitet</param>
    /// <param name="land">Land hvor hjemuniversitetet er</param>
    /// <param name="periodeFra">Startdato for utveksling</param>
    /// <param name="periodeTil">Sluttdato for utveksling</param>
    /// <exception cref="ArgumentException">Hvis periodeFra > periodeTil eller hvis hjemuniversitet/land er tom</exception>
    [JsonConstructor]
    public Utvekslingsstudent(
        string studentID,
        string navn,
        string epost,
        string hjemuniversitet,
        string land,
        DateOnly periodeFra,
        DateOnly periodeTil)
        : base(studentID, navn, epost)
    {
        if (string.IsNullOrWhiteSpace(hjemuniversitet))
            throw new ArgumentException("Hjemuniversitet kan ikke være tom.", nameof(hjemuniversitet));
        if (string.IsNullOrWhiteSpace(land))
            throw new ArgumentException("Land kan ikke være tom.", nameof(land));
        if (periodeFra > periodeTil)
            throw new ArgumentException("PeriodeFra kan ikke være etter PeriodeTil.");

        Hjemuniversitet = hjemuniversitet;
        Land = land;
        PeriodeFra = periodeFra;
        PeriodeTil = periodeTil;
    }

    /// <summary>Returnerer detaljert informasjon om utvekslingsstudenten for visning</summary>
    /// <returns>Tekststreng med utvekslingsdetaljer</returns>
    public override string HentInfo()
    {
        return $"[Utveksling] {StudentID} | {Navn} | {Epost} | {Hjemuniversitet}, {Land} | {PeriodeFra} – {PeriodeTil}";
    }
}
