using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

/// <summary>
/// Håndterer autentiseringsflyt for innlogging og registrering av nye brukere.
/// Vedlikeholder indekser over studenter og ansatte for rask oppslag.
/// </summary>
public class AuthFlow
{
    private readonly AuthService _authService;
    private readonly List<Student> _studenter;
    private readonly List<Ansatt> _ansatte;
    private readonly Dictionary<string, Student> _studenterById;
    private readonly Dictionary<string, Ansatt> _ansatteById;

    /// <summary>Skrivebeskyttet liste over alle studenter i systemet</summary>
    public IReadOnlyList<Student> Studenter => _studenter;
    
    /// <summary>Skrivebeskyttet liste over alle ansatte i systemet</summary>
    public IReadOnlyList<Ansatt> Ansatte => _ansatte;

    /// <summary>
    /// Initialiserer autentiseringsflyt.
    /// </summary>
    /// <param name="authService">Service for autentisering</param>
    /// <param name="studenter">Liste over systemets studenter</param>
    /// <param name="ansatte">Liste over systemets ansatte</param>
    public AuthFlow(AuthService authService, List<Student> studenter, List<Ansatt> ansatte)
    {
        _authService = authService;
        _studenter = studenter;
        _ansatte = ansatte;
        _studenterById = ByggStudentIndeks(_studenter);
        _ansatteById = ByggAnsattIndeks(_ansatte);
    }

    /// <summary>
    /// Forsøker å logge inn bruker med brukernavn og passord.
    /// </summary>
    /// <param name="account">Utparam: logget inn konto hvis vellykket, null ellers</param>
    /// <returns>true hvis innlogging var vellykket</returns>
    public bool TryLogin(out UserAccount? account)
    {
        account = null;

        string username = InputHelper.LesIkkeTom("Brukernavn: ");
        string password = InputHelper.LesIkkeTom("Passord: ");

        var result = _authService.Login(username, password);
        if (!result.IsSuccess || result.Data == null)
        {
            Console.WriteLine(result.Message);
            return false;
        }

        account = result.Data;
        Console.WriteLine(result.Message);
        return true;
    }

    /// <summary>
    /// Interaktiv flyt for å registrere ny bruker.
    /// Spørrer om rolle (student, faglærer eller bibliotekansatt).
    /// </summary>
    public void RegistrerNyBruker()
    {
        Console.WriteLine("\nVelg rolle for ny bruker:");
        Console.WriteLine("[1] Student");
        Console.WriteLine("[2] Faglærer");
        Console.WriteLine("[3] Bibliotekansatt");
        string rolleValg = InputHelper.LesIkkeTom("Velg: ");

        string username = InputHelper.LesIkkeTom("Brukernavn: ");
        string password = InputHelper.LesIkkeTom("Passord (minst 4 tegn): ");

        switch (rolleValg)
        {
            case "1":
            {
                string id = InputHelper.LesIkkeTom("StudentID (format S###): ");
                string navn = InputHelper.LesIkkeTom("Navn: ");
                string epost = LesGyldigEpost();

                var result = _authService.RegisterStudent(username, password, id, navn, epost, _studenter);
                if (!result.IsSuccess || result.Data == null)
                {
                    Console.WriteLine(result.Message);
                    return;
                }

                _studenterById[result.Data.StudentID] = result.Data;
                Console.WriteLine(result.Message);

                break;
            }
            case "2":
            {
                string id = InputHelper.LesIkkeTom("AnsattID (format A###): ");
                string navn = InputHelper.LesIkkeTom("Navn: ");
                string epost = LesGyldigEpost();
                string avdeling = InputHelper.LesIkkeTom("Avdeling: ");

                var result = _authService.RegisterAnsatt(
                    username,
                    password,
                    id,
                    navn,
                    epost,
                    avdeling,
                    StillingType.Foreleser,
                    AppRole.Faglærer,
                    _ansatte);

                if (!result.IsSuccess || result.Data == null)
                {
                    Console.WriteLine(result.Message);
                    return;
                }

                _ansatteById[result.Data.AnsattID] = result.Data;
                Console.WriteLine(result.Message);

                break;
            }
            case "3":
            {
                string id = InputHelper.LesIkkeTom("AnsattID (format A###): ");
                string navn = InputHelper.LesIkkeTom("Navn: ");
                string epost = LesGyldigEpost();
                string avdeling = InputHelper.LesIkkeTom("Avdeling: ");

                var result = _authService.RegisterAnsatt(
                    username,
                    password,
                    id,
                    navn,
                    epost,
                    avdeling,
                    StillingType.Bibliotekar,
                    AppRole.BibliotekAnsatt,
                    _ansatte);

                if (!result.IsSuccess || result.Data == null)
                {
                    Console.WriteLine(result.Message);
                    return;
                }

                _ansatteById[result.Data.AnsattID] = result.Data;
                Console.WriteLine(result.Message);

                break;
            }
            default:
                Console.WriteLine("Ugyldig valg.");
                break;
        }
    }

    /// <summary>
    /// Finner en student basert på e-postkontoen.
    /// </summary>
    /// <param name="account">Brukerkontoen</param>
    /// <returns>Student hvis funnet, null ellers</returns>
    public Student? FinnStudentForAccount(UserAccount account)
    {
        _studenterById.TryGetValue(account.ReferenceId, out var student);
        return student;
    }

    /// <summary>
    /// Finner en ansatt basert på brukerkontoen.
    /// </summary>
    /// <param name="account">Brukerkontoen</param>
    /// <returns>Ansatt hvis funnet, null ellers</returns>
    public Ansatt? FinnAnsattForAccount(UserAccount account)
    {
        _ansatteById.TryGetValue(account.ReferenceId, out var ansatt);
        return ansatt;
    }

    /// <summary>
    /// Leser og validerer en gyldig e-postadresse fra konsolen.
    /// Gjentar spørsmål hvis format er ugyldig.
    /// </summary>
    /// <returns>Gyldig e-postadresse</returns>
    private static string LesGyldigEpost()
    {
        while (true)
        {
            string epost = InputHelper.LesIkkeTom("Epost: ");
            try
            {
                var addr = new System.Net.Mail.MailAddress(epost);
                if (addr.Address == epost && addr.Host.Contains('.'))
                    return epost;
            }
            catch { }

            Console.WriteLine("Ugyldig epost. Prøv igjen.");
        }
    }

    /// <summary>
    /// Bygger indeks (dictionary) over studenter for rask oppslag etter StudentID.
    /// </summary>
    /// <returns>Dictionary: StudentID -> Student</returns>
    private static Dictionary<string, Student> ByggStudentIndeks(IEnumerable<Student> studenter)
    {
        var indeks = new Dictionary<string, Student>(StringComparer.OrdinalIgnoreCase);
        foreach (var student in studenter)
        {
            if (!indeks.ContainsKey(student.StudentID))
            {
                indeks[student.StudentID] = student;
            }
        }

        return indeks;
    }

    /// <summary>
    /// Bygger indeks (dictionary) over ansatte for rask oppslag etter AnsattID.
    /// </summary>
    /// <returns>Dictionary: AnsattID -> Ansatt</returns>
    private static Dictionary<string, Ansatt> ByggAnsattIndeks(IEnumerable<Ansatt> ansatte)
    {
        var indeks = new Dictionary<string, Ansatt>(StringComparer.OrdinalIgnoreCase);
        foreach (var ansatt in ansatte)
        {
            if (!indeks.ContainsKey(ansatt.AnsattID))
            {
                indeks[ansatt.AnsattID] = ansatt;
            }
        }

        return indeks;
    }
}
