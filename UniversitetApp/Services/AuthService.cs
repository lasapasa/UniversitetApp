using UniversitetApp.Models;

namespace UniversitetApp.Services;

public class AuthService
{
    private readonly List<UserAccount> _accounts = new();

    public IReadOnlyList<UserAccount> Accounts => _accounts;

    public OperationResult Register(string username, string password, AppRole role, string referenceId)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(referenceId))
        {
            return OperationResult.Failure("Brukernavn, passord og referanse-ID må fylles ut.", "validation_error");
        }

        if (_accounts.Any(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Failure("Brukernavn er allerede i bruk.", "duplicate_username");
        }

        if (_accounts.Any(a => a.ReferenceId.Equals(referenceId, StringComparison.OrdinalIgnoreCase) && a.Role == role))
        {
            return OperationResult.Failure("Det finnes allerede en konto for denne brukeren i valgt rolle.", "duplicate_reference");
        }

        try
        {
            _accounts.Add(new UserAccount(username, password, role, referenceId));
            return OperationResult.Success("Registrering fullført.");
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Failure(ex.Message, "validation_error");
        }
    }

    public OperationResult<UserAccount> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return OperationResult<UserAccount>.Failure("Brukernavn og passord må fylles ut.", "validation_error");
        }

        var account = _accounts.FirstOrDefault(a =>
            a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
            a.Password == password);

        if (account == null)
        {
            return OperationResult<UserAccount>.Failure("Feil brukernavn eller passord.", "invalid_credentials");
        }

        return OperationResult<UserAccount>.Success("Innlogging vellykket.", account);
    }
}
