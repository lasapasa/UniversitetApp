namespace UniversitetApp.Models;

// Basisklasse for brukere som deler kontaktinformasjon og må gi en egen tekstvisning.
public abstract class Person : IUser
{
    public string Navn { get; private set; }
    public string Epost { get; private set; }

    protected Person(string navn, string epost)
    {
        if (string.IsNullOrWhiteSpace(navn))
            throw new ArgumentException("Navn kan ikke være tom.", nameof(navn));
        if (string.IsNullOrWhiteSpace(epost))
            throw new ArgumentException("Epost kan ikke være tom.", nameof(epost));

        Navn = navn;
        Epost = epost;
    }

    // Hver underklasse bestemmer selv hvordan den presenteres i UI.
    public abstract string HentInfo();

    public override string ToString() => HentInfo();
}
