using UniversitetApp.Models;

namespace UniversitetApp.Services;

// Håndterer bibliotekflyt: registrering, utlån, retur og enkel søking.
public class BibliotekManager
{
    private readonly List<Bok> _bøker = new();
    private readonly Dictionary<string, Bok> _bøkerById = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Laan> _lånHistorikk = new();

    public IReadOnlyList<Bok> HentAlleBøker() => _bøker;
    public List<Laan> HentAktiveLån() => _lånHistorikk.Where(l => l.ErAktivt).ToList();
    public List<Laan> HentHistorikk() => _lånHistorikk.ToList();

    public List<Bok> FinnBøker(string søkeord)
    {
        if (string.IsNullOrWhiteSpace(søkeord)) return _bøker.ToList();

        return _bøker.Where(b =>
            b.Tittel.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.Forfatter.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.MediaID.Contains(søkeord, StringComparison.OrdinalIgnoreCase)).ToList();
    }

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
