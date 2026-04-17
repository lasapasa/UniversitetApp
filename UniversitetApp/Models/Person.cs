namespace UniversitetApp.Models;

/// <summary>
/// Abstrakt basisklasse for alle personer i systemet (studenter og ansatte).
/// Inneholder felles kontaktinformasjon med validering.
/// </summary>
public abstract class Person : IUser
{
    /// <summary>Personens navn</summary>
    public string Navn { get; private set; }
    
    /// <summary>Personens e-postadresse</summary>
    public string Epost { get; private set; }

    /// <summary>
    /// Initialiserer en person med navn og e-post.
    /// </summary>
    /// <param name="navn">Personens navn (kan ikke være tom)</param>
    /// <param name="epost">Personens e-postadresse (kan ikke være tom)</param>
    /// <exception cref="ArgumentException">Hvis navn eller epost er tom</exception>
    protected Person(string navn, string epost)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new ArgumentException("Navn kan ikke være tom.", nameof(navn));
        if (string.IsNullOrWhiteSpace(epost))
            throw new ArgumentException("Epost kan ikke være tom.", nameof(epost));

        Navn = navn;
        Epost = epost;
    }

    /// <summary>
    /// Abstrakt metode: hver underklasse bestemmer selv hvordan den presenteres i UI.
    /// </summary>
    /// <returns>Formatert tekstlig representasjon av personen</returns>
    public abstract string HentInfo();

    /// <summary>Returnerer HentInfo() for konsistent tekstvisning</summary>
    public override string ToString() => HentInfo();
}
