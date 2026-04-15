using System.Text.RegularExpressions;

namespace UniversitetApp.Models;

// Domeneobjekt for ansatte med rolle og organisatorisk tilhørighet.
public class Ansatt : Person
{
    public string AnsattID { get; private set; }
    public StillingType Stilling { get; private set; }
    public string Avdeling { get; private set; }

    public Ansatt(string ansattID, string navn, string epost, StillingType stilling, string avdeling)
        : base(navn, epost)
    {
        if (string.IsNullOrWhiteSpace(ansattID))
            throw new ArgumentException("AnsattID kan ikke være tom.", nameof(ansattID));

        string normalisertId = ansattID.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(normalisertId, "^A\\d{3}$"))
            throw new ArgumentException("AnsattID må ha formatet A### (f.eks. A001).", nameof(ansattID));
        if (string.IsNullOrWhiteSpace(avdeling))
            throw new ArgumentException("Avdeling kan ikke være tom.", nameof(avdeling));

        AnsattID = normalisertId;
        Stilling = stilling;
        Avdeling = avdeling;
    }

    // override av abstrakt metode fra Person
    public override string HentInfo()
    {
        return $"[Ansatt] {AnsattID} | {Navn} | {Epost} | {Stilling} | {Avdeling}";
    }
}
