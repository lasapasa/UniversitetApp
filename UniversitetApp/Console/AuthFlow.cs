using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

public class AuthFlow
{
    private readonly AuthService _authService;
    private readonly List<Student> _studenter;
    private readonly List<Ansatt> _ansatte;
    private readonly Dictionary<string, Student> _studenterById;
    private readonly Dictionary<string, Ansatt> _ansatteById;

    public IReadOnlyList<Student> Studenter => _studenter;
    public IReadOnlyList<Ansatt> Ansatte => _ansatte;

    public AuthFlow(AuthService authService, List<Student> studenter, List<Ansatt> ansatte)
    {
        _authService = authService;
        _studenter = studenter;
        _ansatte = ansatte;
        _studenterById = ByggStudentIndeks(_studenter);
        _ansatteById = ByggAnsattIndeks(_ansatte);
    }

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
                string epost = InputHelper.LesIkkeTom("Epost: ");

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
                string epost = InputHelper.LesIkkeTom("Epost: ");
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
                string epost = InputHelper.LesIkkeTom("Epost: ");
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

    public Student? FinnStudentForAccount(UserAccount account)
    {
        _studenterById.TryGetValue(account.ReferenceId, out var student);
        return student;
    }

    public Ansatt? FinnAnsattForAccount(UserAccount account)
    {
        _ansatteById.TryGetValue(account.ReferenceId, out var ansatt);
        return ansatt;
    }

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
