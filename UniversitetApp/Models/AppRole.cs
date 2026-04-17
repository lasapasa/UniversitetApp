namespace UniversitetApp.Models;

/// <summary>
/// Autorisasjonsroller i systemet.
/// Bestammer hva hver bruker kan gjøre i applikasjonen.
/// </summary>
public enum AppRole
{
    /// <summary>Rolle for studenter</summary>
    Student,
    
    /// <summary>Rolle for undervisere (forelsere)</summary>
    Faglærer,
    
    /// <summary>Rolle for bibliotekansatte</summary>
    BibliotekAnsatt
}
