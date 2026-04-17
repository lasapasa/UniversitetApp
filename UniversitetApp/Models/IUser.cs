namespace UniversitetApp.Models;

/// <summary>
/// Felles kontrakt for alle brukertyper som kan identifiseres og vises i systemet.
/// Implementert av Student og Ansatt for polymorf behandling.
/// </summary>
public interface IUser
{
    /// <summary>Brukerens navn</summary>
    string Navn { get; }
    
    /// <summary>Returnerer en tekstlig representasjon av brukeren for UI</summary>
    /// <returns>Formatert brukerinformasjon</returns>
    string HentInfo();
}
