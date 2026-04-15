using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

public class UniversitetConsoleApp
{
    private readonly KursManager _kursManager = new();
    private readonly BibliotekManager _bibliotekManager = new();
    private readonly AuthService _authService = new();

    private readonly AuthFlow _authFlow;
    private readonly StudentMenu _studentMenu;
    private readonly FaglaererMenu _faglaererMenu;
    private readonly BibliotekMenu _bibliotekMenu;

    public UniversitetConsoleApp()
    {
        SeedDataInitializer.Initialize(_kursManager, _bibliotekManager, _authService, out var studenter, out var ansatte);
        _authFlow = new AuthFlow(_authService, studenter, ansatte);
        _studentMenu = new StudentMenu(_kursManager, _bibliotekManager);
        _faglaererMenu = new FaglaererMenu(_kursManager, _bibliotekManager, studenter);
        _bibliotekMenu = new BibliotekMenu(_bibliotekManager);
    }

    public void Run()
    {
        bool kjør = true;
        while (kjør)
        {
            Console.WriteLine("\n=== Universitetssystem ===");
            Console.WriteLine("[1] Eksisterende bruker");
            Console.WriteLine("[2] Ny bruker (registrer)");
            Console.WriteLine("[0] Avslutt");

            string valg = InputHelper.LesIkkeTom("Velg: ");

            try
            {
                switch (valg)
                {
                    case "1":
                        LoggInnOgKjør();
                        break;
                    case "2":
                        _authFlow.RegistrerNyBruker();
                        break;
                    case "0":
                        kjør = false;
                        Console.WriteLine("Ha det bra.");
                        break;
                    default:
                        Console.WriteLine("Ugyldig valg.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"En uventet feil oppstod: {ex.Message}");
            }
        }
    }

    private void LoggInnOgKjør()
    {
        if (!_authFlow.TryLogin(out var account) || account == null) return;

        if (account.Role == AppRole.Student)
        {
            var student = _authFlow.FinnStudentForAccount(account);
            if (student == null)
            {
                Console.WriteLine("Fant ikke studentprofil for kontoen.");
                return;
            }

            _studentMenu.Run(student);
            return;
        }

        var ansatt = _authFlow.FinnAnsattForAccount(account);
        if (ansatt == null)
        {
            Console.WriteLine("Fant ikke ansattprofil for kontoen.");
            return;
        }

        if (account.Role == AppRole.Faglærer)
        {
            _faglaererMenu.Run(ansatt);
            return;
        }

        if (account.Role == AppRole.BibliotekAnsatt)
        {
            _bibliotekMenu.Run();
            return;
        }

        Console.WriteLine("Ukjent rolle.");
    }
}
