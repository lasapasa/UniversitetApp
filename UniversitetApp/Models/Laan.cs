namespace UniversitetApp.Models;

// Kobler bruker og bok i en utlånstransaksjon med historikk for retur.
public class Laan
{
    public IUser Bruker { get; set; }
    public Bok Bok { get; set; }
    public DateTime LånDato { get; set; }
    public DateTime? ReturDato { get; set; }

    public bool ErAktivt => ReturDato == null;

    public Laan(IUser bruker, Bok bok)
    {
        if (bruker == null) throw new ArgumentNullException(nameof(bruker));
        if (bok == null) throw new ArgumentNullException(nameof(bok));

        Bruker = bruker;
        Bok = bok;
        LånDato = DateTime.UtcNow;
    }

    public void Returner()
    {
        // Setter retur som tidspunkt og markerer lånet som avsluttet.
        ReturDato = DateTime.UtcNow;
    }

    public override string ToString()
    {
        string status = ErAktivt ? "Aktivt" : $"Returnert {ReturDato:dd.MM.yyyy}";
        return $"{Bruker.Navn} — \"{Bok.Tittel}\" | Lånt: {LånDato:dd.MM.yyyy} | {status}";
    }
}
