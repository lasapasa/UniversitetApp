using System.Text.RegularExpressions;

namespace UniversitetApp.Models;

/// <summary>
/// Representerer en ansatt (lærer eller bibliotekansatte) i systemet.
/// Inneholder stil...og og organisatorisk tilhørighet.
/// AnsattID valideres mot formatet A###.
/// </summary>
public class Ansatt : Person
{
    /// <summary>Unik identifikator for ansatt (format: A###)</summary>
    public string AnsattID { get; private set; }
    
    /// <summary>Stillingskategorien for denne ansatte</summary>
    public StillingType Stilling { get; private set; }
    
    /// <summary>Organisatorisk avdeling som ansatte tilhører</summary>
    public string Avdeling { get; private set; }

    /// <summary>
    /// Initialiserer en ny ansatt med validering av AnsattID-format og avdelingsinformasjon.
    /// </summary>
    /// <param name="ansattID">Ansatt-ID i format A### (f.eks. A001)</param>
    /// <param name="navn">Ansattes navn</param>
    /// <param name="epost">Ansattes e-postadresse</param>
    /// <param name="stilling">Stillingskategori (Foreleser eller Bibliotekar)</param>
    /// <param name="avdeling">Avdeling eller institusjon som ansatte tilhører</param>
    /// <exception cref="ArgumentException">Hvis AnsattID ikke har korrekt format eller avdeling er tom</exception>
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

    /// <summary>Returnerer formatert informasjon om ansatte for visning</summary>
    /// <returns>Tekststreng med ansatt-detaljer</returns>
    public override string HentInfo()
    {
        return $"[Ansatt] {AnsattID} | {Navn} | {Epost} | {Stilling} | {Avdeling}";
    }
}
