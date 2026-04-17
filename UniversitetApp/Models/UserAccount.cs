namespace UniversitetApp.Models;

/// <summary>
/// Representerer en brukerkonto i systemet.
/// Inneholder autentiseringsdetaljer og rolle-informasjon.
/// </summary>
public class UserAccount
{
    /// <summary>Brukernavn (case-insensitive)</summary>
    public string Username { get; }
    
    /// <summary>Passord (minimum 4 tegn)</summary>
    public string Password { get; }
    
    /// <summary>Brukerens rolle (Student, Faglærer eller BibliotekAnsatt)</summary>
    public AppRole Role { get; }
    
    /// <summary>Referanse-ID som linker til StudentID eller AnsattID</summary>
    public string ReferenceId { get; }

    /// <summary>
    /// Initialiserer en ny brukerkonto med validering av felt.
    /// </summary>
    /// <param name="username">Brukernavn (må være unik)</param>
    /// <param name="password">Passord (minimum 4 tegn)</param>
    /// <param name="role">Brukerens rolle i systemet</param>
    /// <param name="referenceId">Link til Student-ID eller Ansatt-ID</param>
    /// <exception cref="ArgumentException">Hvis noen felt ikke oppfyller requirements</exception>
    public UserAccount(string username, string password, AppRole role, string referenceId)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Brukernavn kan ikke være tom.", nameof(username));
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            throw new ArgumentException("Passord må være minst 4 tegn.", nameof(password));
        if (string.IsNullOrWhiteSpace(referenceId))
            throw new ArgumentException("ReferenceId kan ikke være tom.", nameof(referenceId));

        Username = username.Trim();
        Password = password;
        Role = role;
        ReferenceId = referenceId.Trim();
    }
}
