namespace UniversitetApp.Models;

/// <summary>
/// Representerer en bok (bibliotekmedium) i systemet.
/// Inneholder metadata og lagerstatus for utlån.
/// </summary>
public class Bok
{
    /// <summary>Unik media-ID for boken</summary>
    public string MediaID { get; private set; }
    
    /// <summary>Boktittel</summary>
    public string Tittel { get; private set; }
    
    /// <summary>Bokforfatter</summary>
    public string Forfatter { get; private set; }
    
    /// <summary>Utgivelsesår</summary>
    public int År { get; private set; }
    
    /// <summary>Totalt antall eksemplarer i biblioteket</summary>
    public int AntallEksemplarer { get; private set; }
    
    /// <summary>Antall eksemplarer som er tilgjengelige for utlån</summary>
    public int TilgjengeligeEksemplarer { get; set; }

    /// <summary>Beregnet: om minst ett eksemplar er tilgjengelig</summary>
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
