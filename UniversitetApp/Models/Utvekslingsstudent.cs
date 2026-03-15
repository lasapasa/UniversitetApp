using System.Text.Json.Serialization;

namespace UniversitetApp.Models;

// Spesialisert studenttype med informasjon om utvekslingsopphold.
public class Utvekslingsstudent : Student
{
    public string Hjemuniversitet { get; set; }
    public string Land { get; set; }
    public DateOnly PeriodeFra { get; set; }
    public DateOnly PeriodeTil { get; set; }

    // Brukes ved (de)serialisering slik at utvekslingsdata leses direkte fra JSON.
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

    // Overstyrt visning gir mer detaljert info for utvekslingsstudenter.
    public override string HentInfo()
    {
        return $"[Utveksling] {StudentID} | {Navn} | {Epost} | {Hjemuniversitet}, {Land} | {PeriodeFra} – {PeriodeTil}";
    }
}
