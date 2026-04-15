using UniversitetApp.Models;

namespace UniversitetApp.Services;

// Håndterer bibliotekflyt: registrering, utlån, retur og enkel søking.
public class BibliotekManager
{
    private readonly List<Bok> _bøker = new();
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

        if (_bøker.Any(b => b.MediaID.Equals(mediaID, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Failure($"Bok med ID '{mediaID}' finnes allerede.", "duplicate_media_id");
        }

        try
        {
            _bøker.Add(new Bok(mediaID, tittel, forfatter, år, antallEksemplarer));
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

    public void VisAktiveLån()
    {
        var aktive = HentAktiveLån();

        if (aktive.Count == 0)
        {
            Console.WriteLine("Ingen aktive lån.");
            return;
        }

        Console.WriteLine($"\nAktive lån ({aktive.Count}):");
        foreach (var lån in aktive)
            Console.WriteLine($"  {lån}");
    }

    public void VisHistorikk()
    {
        if (_lånHistorikk.Count == 0)
        {
            Console.WriteLine("Ingen lånhistorikk.");
            return;
        }

        Console.WriteLine($"\nLånhistorikk ({_lånHistorikk.Count} totalt):");
        foreach (var lån in _lånHistorikk)
            Console.WriteLine($"  {lån}");
    }

    public void SøkEtterBok(string søkeord)
    {
        var treff = FinnBøker(søkeord);

        if (treff.Count == 0)
        {
            Console.WriteLine($"Ingen bøker funnet for '{søkeord}'.");
            return;
        }

        Console.WriteLine($"Fant {treff.Count} bok(er):");
        foreach (var bok in treff)
            Console.WriteLine($"  {bok}");
    }

    private Bok? FinnBok(string mediaID, out string feil)
    {
        feil = string.Empty;
        var bok = _bøker.FirstOrDefault(b => b.MediaID.Equals(mediaID, StringComparison.OrdinalIgnoreCase));
        if (bok == null)
            feil = $"Fant ikke bok med ID '{mediaID}'.";
        return bok;
    }

}
