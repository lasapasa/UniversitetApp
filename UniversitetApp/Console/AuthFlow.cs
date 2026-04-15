using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

public class AuthFlow
{
    private readonly AuthService _authService;
    private readonly List<Student> _studenter;
    private readonly List<Ansatt> _ansatte;

    public AuthFlow(AuthService authService, List<Student> studenter, List<Ansatt> ansatte)
    {
        _authService = authService;
        _studenter = studenter;
        _ansatte = ansatte;
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

                if (_studenter.Any(s => s.StudentID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("StudentID finnes allerede.");
                    return;
                }

                try
                {
                    var student = new Student(id, navn, epost);
                    var registerResult = _authService.Register(username, password, AppRole.Student, student.StudentID);
                    if (!registerResult.IsSuccess)
                    {
                        Console.WriteLine(registerResult.Message);
                        return;
                    }

                    _studenter.Add(student);
                    Console.WriteLine("Ny student registrert.");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ugyldig input: {ex.Message}");
                }

                break;
            }
            case "2":
            {
                string id = InputHelper.LesIkkeTom("AnsattID (format A###): ");
                string navn = InputHelper.LesIkkeTom("Navn: ");
                string epost = InputHelper.LesIkkeTom("Epost: ");
                string avdeling = InputHelper.LesIkkeTom("Avdeling: ");

                if (_ansatte.Any(a => a.AnsattID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("AnsattID finnes allerede.");
                    return;
                }

                try
                {
                    var ansatt = new Ansatt(id, navn, epost, StillingType.Foreleser, avdeling);
                    var registerResult = _authService.Register(username, password, AppRole.Faglærer, ansatt.AnsattID);
                    if (!registerResult.IsSuccess)
                    {
                        Console.WriteLine(registerResult.Message);
                        return;
                    }

                    _ansatte.Add(ansatt);
                    Console.WriteLine("Ny faglærer registrert.");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ugyldig input: {ex.Message}");
                }

                break;
            }
            case "3":
            {
                string id = InputHelper.LesIkkeTom("AnsattID (format A###): ");
                string navn = InputHelper.LesIkkeTom("Navn: ");
                string epost = InputHelper.LesIkkeTom("Epost: ");
                string avdeling = InputHelper.LesIkkeTom("Avdeling: ");

                if (_ansatte.Any(a => a.AnsattID.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("AnsattID finnes allerede.");
                    return;
                }

                try
                {
                    var ansatt = new Ansatt(id, navn, epost, StillingType.Bibliotekar, avdeling);
                    var registerResult = _authService.Register(username, password, AppRole.BibliotekAnsatt, ansatt.AnsattID);
                    if (!registerResult.IsSuccess)
                    {
                        Console.WriteLine(registerResult.Message);
                        return;
                    }

                    _ansatte.Add(ansatt);
                    Console.WriteLine("Ny bibliotekansatt registrert.");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ugyldig input: {ex.Message}");
                }

                break;
            }
            default:
                Console.WriteLine("Ugyldig valg.");
                break;
        }
    }

    public Student? FinnStudentForAccount(UserAccount account)
    {
        return _studenter.FirstOrDefault(s => s.StudentID.Equals(account.ReferenceId, StringComparison.OrdinalIgnoreCase));
    }

    public Ansatt? FinnAnsattForAccount(UserAccount account)
    {
        return _ansatte.FirstOrDefault(a => a.AnsattID.Equals(account.ReferenceId, StringComparison.OrdinalIgnoreCase));
    }
}
