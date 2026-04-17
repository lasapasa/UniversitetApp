namespace UniversitetApp.Models;

/// <summary>
/// Representerer en utlånstransaksjon mellom bruker og bok.
/// Inneholder låne- og returopp...mmer med tidspunkter.
/// </summary>
public class Laan
{
    /// <summary>Brukeren som har lånt boken (Student eller Ansatt)</summary>
    public IUser Bruker { get; set; }
    
    /// <summary>Boken som er lånt ut</summary>
    public Bok Bok { get; set; }
    
    /// <summary>Tidspunkt når boken ble lånt ut</summary>
    public DateTime LånDato { get; set; }
    
    /// <summary>Tidspunkt når boken ble returnert (null hvis fortsatt utlånt)</summary>
    public DateTime? ReturDato { get; set; }

    /// <summary>Beregnet: om lånet er aktivt (ikke returnert)</summary>
    public bool ErAktivt => ReturDato == null;

    /// <summary>
    /// Initialiserer et nytt lån.
    /// LånDato settes automatisk til nå.
    /// </summary>
    /// <param name="bruker">Brukeren som låner boken</param>
    /// <param name="bok">Boken som lånes</param>
    /// <exception cref="ArgumentNullException">Hvis bruker eller bok er null</exception>
    public Laan(IUser bruker, Bok bok)
    {
        if (bruker == null) throw new ArgumentNullException(nameof(bruker));
        if (bok == null) throw new ArgumentNullException(nameof(bok));

        Bruker = bruker;
        Bok = bok;
        LånDato = DateTime.UtcNow;
    }

    /// <summary>
    /// Markerer lånet som returnert ved å sette ReturDato til nå.
    /// </summary>
    public void Returner()
    {
        ReturDato = DateTime.UtcNow;
    }

    public override string ToString()
    {
        string status = ErAktivt ? "Aktivt" : $"Returnert {ReturDato:dd.MM.yyyy}";
        return $"{Bruker.Navn} — \"{Bok.Tittel}\" | Lånt: {LånDato:dd.MM.yyyy} | {status}";
    }
}
