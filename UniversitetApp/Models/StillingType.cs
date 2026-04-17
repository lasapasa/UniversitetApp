namespace UniversitetApp.Models;

/// <summary>
/// Stillingskategorier for ansatte i systemet.
/// Brukes til å klassifisere ansattes rolle og oppgaver.
/// </summary>
public enum StillingType
{
    /// <summary>Undervisende stilling - ansette som underviser i fag</summary>
    Foreleser,
    
    /// <summary>Bibliotekstilling - ansatte som administrerer biblioteket</summary>
    Bibliotekar
}