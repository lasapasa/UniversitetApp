namespace UniversitetApp.Models;

// Felles kontrakt for alle brukertyper som kan identifiseres og vises i systemet.
public interface IUser
{
    string Navn { get; }
    string HentInfo();
}
