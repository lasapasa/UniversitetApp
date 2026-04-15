namespace UniversitetApp.Models;

public class UserAccount
{
    public string Username { get; }
    public string Password { get; }
    public AppRole Role { get; }
    public string ReferenceId { get; }

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
