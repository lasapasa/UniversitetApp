using UniversitetApp.Models;

namespace UniversitetApp.Services;

/// <summary>
/// Håndterer autentisering og kontoregistrering for både studenter og ansatte.
/// Vedlikeholder både liste og indeks av kontoer for effektiv oppslag.
/// </summary>
public class AuthService
{
    private readonly List<UserAccount> _accounts = new();
    private readonly Dictionary<string, UserAccount> _accountsByUsername = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returnerer en skrivebeskyttet liste over alle kontoer.
    /// </summary>
    public IReadOnlyList<UserAccount> Accounts => _accounts;

    /// <summary>
    /// Registrerer en ny konto med validering.
    /// Sikrer at brukernavn er unikt og at referansen ikke allerede er registrert med samme rolle.
    /// </summary>
    /// <param name="username">Brukernavn (må være unikt)</param>
    /// <param name="password">Passord for kontoen</param>
    /// <param name="role">Rolle som Student, Faglærer eller BibliotekAnsatt</param>
    /// <param name="referenceId">Referanse-ID (StudentID eller AnsattID)</param>
    /// <returns>Resultat som indikerer om registrering var vellykket</returns>
    public OperationResult Register(string username, string password, AppRole role, string referenceId)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(referenceId))
        {
            return OperationResult.Failure("Brukernavn, passord og referanse-ID må fylles ut.", "validation_error");
        }

        string normalisertBrukernavn = NormaliserBrukernavn(username);
        if (_accountsByUsername.ContainsKey(normalisertBrukernavn))
        {
            return OperationResult.Failure("Brukernavn er allerede i bruk.", "duplicate_username");
        }

        if (_accounts.Any(a => a.ReferenceId.Equals(referenceId, StringComparison.OrdinalIgnoreCase) && a.Role == role))
        {
            return OperationResult.Failure("Det finnes allerede en konto for denne brukeren i valgt rolle.", "duplicate_reference");
        }

        try
        {
            var account = new UserAccount(username, password, role, referenceId);
            LeggTilKontoInternt(account);
            return OperationResult.Success("Registrering fullført.");
        }
        catch (ArgumentException ex)
        {
            return OperationResult.Failure(ex.Message, "validation_error");
        }
    }

    /// <summary>
    /// Registrerer en ny student og oppretter konto for studenten.
    /// Validerer at StudentID ikke allerede finnes.
    /// </summary>
    /// <param name="username">Brukernavn for kontoen</param>
    /// <param name="password">Passord for kontoen</param>
    /// <param name="studentId">Student-ID (må være unik)</param>
    /// <param name="navn">Student navn</param>
    /// <param name="epost">Student e-post</param>
    /// <param name="studenter">Koleksjon av eksisterende studenter</param>
    /// <returns>Resultat med ny student hvis vellykket</returns>
    public OperationResult<Student> RegisterStudent(
        string username,
        string password,
        string studentId,
        string navn,
        string epost,
        ICollection<Student> studenter)
    {
        if (studenter.Any(s => s.StudentID.Equals(studentId, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult<Student>.Failure("StudentID finnes allerede.", "duplicate_student_id");
        }

        Student student;
        try
        {
            student = new Student(studentId, navn, epost);
        }
        catch (ArgumentException ex)
        {
            return OperationResult<Student>.Failure($"Ugyldig input: {ex.Message}", "validation_error");
        }

        var accountResult = Register(username, password, AppRole.Student, student.StudentID);
        if (!accountResult.IsSuccess)
        {
            return OperationResult<Student>.Failure(accountResult.Message, accountResult.ErrorCode);
        }

        studenter.Add(student);
        return OperationResult<Student>.Success("Ny student registrert.", student);
    }

    /// <summary>
    /// Registrerer en ny ansatt og oppretter konto med riktig rolle basert på stilling.
    /// Validerer at AnsattID ikke allerede finnes.
    /// </summary>
    /// <param name="username">Brukernavn for kontoen</param>
    /// <param name="password">Passord for kontoen</param>
    /// <param name="ansattId">Ansatt-ID (må være unik)</param>
    /// <param name="navn">Ansatt navn</param>
    /// <param name="epost">Ansatt e-post</param>
    /// <param name="avdeling">Ansatt avdeling</param>
    /// <param name="stilling">Stillingtype (Foreleser, etc.)</param>
    /// <param name="role">Rolle som Faglærer eller BibliotekAnsatt</param>
    /// <param name="ansatte">Koleksjon av eksisterende ansatte</param>
    /// <returns>Resultat med ny ansatt hvis vellykket</returns>
    public OperationResult<Ansatt> RegisterAnsatt(
        string username,
        string password,
        string ansattId,
        string navn,
        string epost,
        string avdeling,
        StillingType stilling,
        AppRole role,
        ICollection<Ansatt> ansatte)
    {
        if (ansatte.Any(a => a.AnsattID.Equals(ansattId, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult<Ansatt>.Failure("AnsattID finnes allerede.", "duplicate_employee_id");
        }

        Ansatt ansatt;
        try
        {
            ansatt = new Ansatt(ansattId, navn, epost, stilling, avdeling);
        }
        catch (ArgumentException ex)
        {
            return OperationResult<Ansatt>.Failure($"Ugyldig input: {ex.Message}", "validation_error");
        }

        var accountResult = Register(username, password, role, ansatt.AnsattID);
        if (!accountResult.IsSuccess)
        {
            return OperationResult<Ansatt>.Failure(accountResult.Message, accountResult.ErrorCode);
        }

        ansatte.Add(ansatt);
        return OperationResult<Ansatt>.Success(
            role == AppRole.Faglærer ? "Ny faglærer registrert." : "Ny bibliotekansatt registrert.",
            ansatt);
    }

    public OperationResult<UserAccount> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return OperationResult<UserAccount>.Failure("Brukernavn og passord må fylles ut.", "validation_error");
        }

        _accountsByUsername.TryGetValue(NormaliserBrukernavn(username), out var account);

        if (account == null || account.Password != password)
        {
            return OperationResult<UserAccount>.Failure("Feil brukernavn eller passord.", "invalid_credentials");
        }

        return OperationResult<UserAccount>.Success("Innlogging vellykket.", account);
    }

    public void LastInnKontoer(IEnumerable<UserAccount> kontoer)
    {
        _accounts.Clear();
        _accountsByUsername.Clear();

        foreach (var konto in kontoer)
        {
            string brukernavn = NormaliserBrukernavn(konto.Username);
            if (_accountsByUsername.ContainsKey(brukernavn))
            {
                continue;
            }

            LeggTilKontoInternt(konto);
        }
    }

    private void LeggTilKontoInternt(UserAccount account)
    {
        _accounts.Add(account);
        _accountsByUsername[NormaliserBrukernavn(account.Username)] = account;
    }

    private static string NormaliserBrukernavn(string brukernavn)
    {
        return brukernavn.Trim();
    }
}
