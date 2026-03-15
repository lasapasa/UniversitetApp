using UniversitetApp.Models;

namespace UniversitetApp.Services;

// Håndterer bibliotekflyt: registrering, utlån, retur og enkel søking.
public class BibliotekManager
{
    private List<Bok> _bøker = new();
    private List<Laan> _lånHistorikk = new();

    public void RegistrerBok(string mediaID, string tittel, string forfatter, int år, int antallEksemplarer)
    {
        if (string.IsNullOrWhiteSpace(mediaID) || string.IsNullOrWhiteSpace(tittel) || string.IsNullOrWhiteSpace(forfatter))
        {
            Console.WriteLine("Feil: Media-ID, tittel og forfatter må fylles ut.");
            return;
        }

        if (år <= 0 || antallEksemplarer <= 0)
        {
            Console.WriteLine("Feil: Utgivelsesår og antall eksemplarer må være større enn 0.");
            return;
        }

        if (_bøker.Any(b => b.MediaID == mediaID))
        {
            Console.WriteLine($"Feil: Bok med ID '{mediaID}' finnes allerede.");
            return;
        }

        _bøker.Add(new Bok(mediaID, tittel, forfatter, år, antallEksemplarer));
        Console.WriteLine($"Bok registrert: \"{tittel}\" av {forfatter}");
    }

    public void LånUtBok(IUser bruker, string mediaID)
    {
        if (string.IsNullOrWhiteSpace(mediaID))
        {
            Console.WriteLine("Feil: Bok-ID kan ikke være tom.");
            return;
        }

        // Stopper tidlig hvis bok ikke finnes eller ikke kan lånes ut.
        var bok = FinnBok(mediaID);
        if (bok == null) return;

        if (!bok.ErTilgjengelig)
        {
            Console.WriteLine($"Feil: \"{bok.Tittel}\" er ikke tilgjengelig for utlån.");
            return;
        }

        bok.TilgjengeligeEksemplarer--;
        var lån = new Laan(bruker, bok);
        _lånHistorikk.Add(lån);
        Console.WriteLine($"{bruker.Navn} har lånt \"{bok.Tittel}\". Tilgjengelige eksemplarer: {bok.TilgjengeligeEksemplarer}");
    }

    public void ReturnerBok(IUser bruker, string mediaID)
    {
        if (string.IsNullOrWhiteSpace(mediaID))
        {
            Console.WriteLine("Feil: Bok-ID kan ikke være tom.");
            return;
        }

        // Finner aktivt lån for riktig bruker og medie-ID.
        var aktivtLån = _lånHistorikk.FirstOrDefault(l =>
            l.ErAktivt &&
            l.Bruker == bruker &&
            l.Bok.MediaID.Equals(mediaID, StringComparison.OrdinalIgnoreCase));

        if (aktivtLån == null)
        {
            Console.WriteLine($"Feil: Fant ikke aktivt lån for denne boken og brukeren.");
            return;
        }

        aktivtLån.Returner();
        aktivtLån.Bok.TilgjengeligeEksemplarer++;
        Console.WriteLine($"{bruker.Navn} har returnert \"{aktivtLån.Bok.Tittel}\".");
    }

    public void VisAktiveLån()
    {
        var aktive = _lånHistorikk.Where(l => l.ErAktivt).ToList();

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
        var treff = _bøker.Where(b =>
            b.Tittel.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.Forfatter.Contains(søkeord, StringComparison.OrdinalIgnoreCase) ||
            b.MediaID.Contains(søkeord, StringComparison.OrdinalIgnoreCase)).ToList();

        if (treff.Count == 0)
        {
            Console.WriteLine($"Ingen bøker funnet for '{søkeord}'.");
            return;
        }

        Console.WriteLine($"Fant {treff.Count} bok(er):");
        foreach (var bok in treff)
            Console.WriteLine($"  {bok}");
    }

    private Bok? FinnBok(string mediaID)
    {
        var bok = _bøker.FirstOrDefault(b => b.MediaID.Equals(mediaID, StringComparison.OrdinalIgnoreCase));
        if (bok == null)
            Console.WriteLine($"Feil: Fant ikke bok med ID '{mediaID}'.");
        return bok;
    }

}
