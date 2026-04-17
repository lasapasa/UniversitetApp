using UniversitetApp.Models;

namespace UniversitetApp.Services;

/// <summary>
/// Håndterer bibliotekflyt: registrering, utlån, retur og søking etter bøker.
/// Vedlikeholder både liste og indeks av bøker, pluss utlånhistorikk.
/// </summary>
public class BibliotekManager
{
    private readonly List<Bok> _bøker = new();
    private readonly Dictionary<string, Bok> _bøkerById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Laan> _lånHistorikk = new();

    /// <summary>
    /// Returnerer en skrivebeskyttet liste over alle registrerte bøker.
    /// </summary>
    public IReadOnlyList<Bok> HentAlleBøker() => _bøker;

    /// <summary>
    /// Returnerer liste over alle aktive (ikke returnerte) lån.
    /// </summary>
    public List<Laan> HentAktiveLån() => _lånHistorikk.Where(l => l.ErAktivt).ToList();

    /// <summary>
    /// Returnerer fullstendig utlånhistorikk (både aktive og returnerte lån).
    /// </summary>
    public List<Laan> HentHistorikk() => _lånHistorikk.ToList();

    /// <summary>
    /// Søker etter bøker basert på tittel, forfatter eller media-ID.
    /// Returner alle bøker hvis søkeord er tomt.
    /// </summary>
    /// <param name="søkeord">Søkord som skal matches (case-insensitive)</param>
    /// <returns>Liste over matchende bøker</returns>
    public List<Bok> FinnBøker(string søkeord)
    {
        if (string.IsNullOrWhiteSpace(søkeord)) return _bøker.ToList();

        return _bøker.Where(b =>
            b.Tittel.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.Forfatter.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.MediaID.Contains(søkeord, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Registrerer en ny bok i biblioteket med validering.
    /// Sikrer at media-ID er unik og at utgivelsesår er realistisk.
    /// </summary>
    /// <param name="mediaID">Unik media-ID for boken</param>
    /// <param name="tittel">Boktittel</param>
    /// <param name="forfatter">Bokforfatter</param>
    /// <param name="år">Utgivelsesår (mellom 1450 og inneværende år)</param>
    /// <param name="antallEksemplarer">Antall fysiske eksemplarer i biblioteket</param>
    /// <returns>Resultat som indikerer om registrering var vellykket</returns>
    public OperationResult RegistrerBok(string mediaID, string tittel, string forfatter, int år, int antallEksemplarer)
    {
        if (string.IsNullOrWhiteSpace(mediaID) || string.IsNullOrWhiteSpace(tittel) || string.IsNullOrWhiteSpace(forfatter))
        {
            return OperationResult.Failure("Media-ID, tittel og forfatter må fylles ut.", "validation_error");
        }

        int inneværendeÅr = DateTime.Now.Year;
        if (år < 1450 || år > inneværendeÅr)
        {
            return OperationResult.Failure($"Utgivelsesår må være mellom 1450 og {inneværendeÅr}.", "validation_error");
        }

        if (antallEksemplarer <= 0)
        {
            return OperationResult.Failure("Antall eksemplarer må være større enn 0.", "validation_error");
        }

        string normalisertMediaId = NormaliserMediaId(mediaID);
        if (_bøkerById.ContainsKey(normalisertMediaId))
        {
            return OperationResult.Failure($"Bok med ID '{mediaID}' finnes allerede.", "duplicate_media_id");
        }

        try
        {
    /// <summary>
    /// Låner ut en bok til en bruker (student eller ansatt).
    /// Validerer at boken eksisterer og har tilgjengelige eksemplarer.
    /// </summary>
    /// <param name="bruker">Bruker som skal låne boken</param>
    /// <param name="mediaID">Media-ID på boken som skal lånes ut</param>
    /// <returns>Resultat med statusmelding</returns>
            var bok = new Bok(mediaID, tittel, forfatter, år, antallEksemplarer);
            LeggTilBokInternt(bok);
            return OperationResult.Success($"Bok registrert: \"{tittel}\" av {forfatter}");
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Failure(ex.Message, "validation_error");
        }
    }

    public OperationResult LånUtBok(IUser bruker, string mediaID)
    {
        if (string.IsNullOrWhiteSpace(mediaID))
        {
            return OperationResult.Failure("Bok-ID kan ikke være tom.", "validation_error");
        }

        var bok = FinnBok(mediaID, out var finnFeil);
        if (bok == null)
        {
            return OperationResult.Failure(finnFeil, "book_not_found");
        }

        if (!bok.ErTilgjengelig)
        {
            return OperationResult.Failure($"\"{bok.Tittel}\" er ikke tilgjengelig for utlån.", "book_unavailable");
        }
/// <summary>
    /// Returnerer en utlånt bok.
    /// Marker lånet som avsluttet og øker antall tilgjengelige eksemplarer.
    /// Validerer at bruker har aktivt lån på boken.
    /// </summary>
    /// <param name="bruker">Bruker som returnerer boken</param>
    /// <param name="mediaID">Media-ID på boken som skal returneres</param>
    /// <returns>Resultat med statusmelding</returns>
    
        bok.TilgjengeligeEksemplarer--;
        var lån = new Laan(bruker, bok);
        _lånHistorikk.Add(lån);
        return OperationResult.Success($"{bruker.Navn} har lånt \"{bok.Tittel}\". Tilgjengelige eksemplarer: {bok.TilgjengeligeEksemplarer}");
    }

    public OperationResult ReturnerBok(IUser bruker, string mediaID)
    {
        if (string.IsNullOrWhiteSpace(mediaID))
        {
            return OperationResult.Failure("Bok-ID kan ikke være tom.", "validation_error");
        }

        var aktivtLån = _lånHistorikk.FirstOrDefault(l =>
            l.ErAktivt &&
            l.Bruker == bruker &&
            l.Bok.MediaID.Equals(mediaID, StringComparison.OrdinalIgnoreCase));

        if (aktivtLån == null)
        {
            return OperationResult.Failure("Fant ikke aktivt lån for denne boken og brukeren.", "active_loan_not_found");
        }

        aktivtLån.Returner();
        aktivtLån.Bok.TilgjengeligeEksemplarer++;
        return OperationResult.Success($"{bruker.Navn} har returnert \"{aktivtLån.Bok.Tittel}\".");
    }

    /// <summary>
    /// Laster inn bøker og lånhistorikk fra en ekstern kilde.
    /// Tømmer eksisterende data før innlasting og bygger indeksen på nytt.
    /// </summary>
    /// <param name="bøker">Samling av bøker som skal lastes inn</param>
    /// <param name="lånHistorikk">Samling av lånregistreringer som skal lastes inn</param>
    private Bok? FinnBok(string mediaID, out string feil)
    {
        feil = string.Empty;
        if (string.IsNullOrWhiteSpace(mediaID))
        {
            feil = "Bok-ID kan ikke være tom.";
            return null;
        }

        _bøkerById.TryGetValue(NormaliserMediaId(mediaID), out var bok);
        if (bok == null)
            feil = $"Fant ikke bok med ID '{mediaID}'.";
        return bok;
    }

    public void LastInnData(IEnumerable<Bok> bøker, IEnumerable<Laan> lånHistorikk)
    {
        _bøker.Clear();
        _bøkerById.Clear();
        _lånHistorikk.Clear();

        foreach (var bok in bøker)
        {
            string mediaId = NormaliserMediaId(bok.MediaID);
            if (_bøkerById.ContainsKey(mediaId))
            {
                continue;
            }

            LeggTilBokInternt(bok);
        }

        _lånHistorikk.AddRange(lånHistorikk);
    }

    private void LeggTilBokInternt(Bok bok)
    {
        _bøker.Add(bok);
        _bøkerById[NormaliserMediaId(bok.MediaID)] = bok;
    }

    private static string NormaliserMediaId(string mediaID)
    {
        return mediaID.Trim().ToUpperInvariant();
    }

}
