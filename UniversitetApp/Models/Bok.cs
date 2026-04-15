namespace UniversitetApp.Models;

// Representerer et bibliotekmedium med lagerstatus for utlån.
public class Bok
{
    public string MediaID { get; private set; }
    public string Tittel { get; private set; }
    public string Forfatter { get; private set; }
    public int År { get; private set; }
    public int AntallEksemplarer { get; private set; }
    public int TilgjengeligeEksemplarer { get; set; }

    public bool ErTilgjengelig => TilgjengeligeEksemplarer > 0;

    public Bok(string mediaID, string tittel, string forfatter, int år, int antallEksemplarer)
    {
        if (string.IsNullOrWhiteSpace(mediaID) || string.IsNullOrWhiteSpace(tittel) || string.IsNullOrWhiteSpace(forfatter))
            throw new ArgumentException("Media-ID, tittel og forfatter må fylles ut.");
        if (år <= 0)
            throw new ArgumentException("Utgivelsesår må være større enn 0.", nameof(år));
        if (antallEksemplarer <= 0)
            throw new ArgumentException("Antall eksemplarer må være større enn 0.", nameof(antallEksemplarer));

        MediaID = mediaID.Trim().ToUpperInvariant();
        Tittel = tittel;
        Forfatter = forfatter;
        År = år;
        AntallEksemplarer = antallEksemplarer;
        TilgjengeligeEksemplarer = antallEksemplarer;
    }

    public override string ToString()
    {
        // Gir en kompakt visning som kan brukes direkte i søkeresultater.
        string status = ErTilgjengelig ? $"Tilgjengelig ({TilgjengeligeEksemplarer})" : "Ikke tilgjengelig";
        return $"{MediaID} | {Tittel} | {Forfatter} | {År} | {status}";
    }
}
