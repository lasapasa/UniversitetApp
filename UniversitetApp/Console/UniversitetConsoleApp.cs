using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

/// <summary>
/// Hovedklasse for universitetssystemet.
/// Håndterer initialisering, datainnlasting/lagring, og bruker-menyvalg.
/// </summary>
public class UniversitetConsoleApp
{
    private readonly KursManager _kursManager = new();
    private readonly BibliotekManager _bibliotekManager = new();
    private readonly AuthService _authService = new();
    private readonly string _dataFilsti;

    private readonly AuthFlow _authFlow;
    private readonly StudentMenu _studentMenu;
    private readonly FaglaererMenu _faglaererMenu;
    private readonly BibliotekMenu _bibliotekMenu;

    /// <summary>
    /// Initialiserer applikasjonen.
    /// Laster data fra fil hvis den eksisterer, ellers bruker eksempeldata.
    /// </summary>
    public UniversitetConsoleApp()
    {
        _dataFilsti = FinnDataFilsti();
        var lastResult = AppStateStore.LastInn(_dataFilsti);

        List<Student> studenter;
        List<Ansatt> ansatte;
        if (lastResult.IsSuccess && lastResult.Data != null)
        {
            var data = lastResult.Data;
            studenter = data.Studenter;
            ansatte = data.Ansatte;
            _kursManager.LastInnKurs(data.Kurs);
            _bibliotekManager.LastInnData(data.Boker, data.LaanHistorikk);
            _authService.LastInnKontoer(data.Accounts);
            Console.WriteLine($"[Lastet data fra {_dataFilsti}]");
        }
        else
        {
            SeedDataInitializer.Initialize(_kursManager, _bibliotekManager, _authService, out studenter, out ansatte);
            Console.WriteLine("[Bruker eksempeldata for denne økten]");
        }

        _authFlow = new AuthFlow(_authService, studenter, ansatte);
        _studentMenu = new StudentMenu(_kursManager, _bibliotekManager);
        _faglaererMenu = new FaglaererMenu(_kursManager, _bibliotekManager, studenter);
        _bibliotekMenu = new BibliotekMenu(_bibliotekManager);
    }

    /// <summary>
    /// Kjører hovedmeny-løkka av applikasjonen.
    /// Bruker kan velge å logge inn, registrere ny bruker, eller avslutte.
    /// </summary>
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
                        LagreDataTilFil();
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

    /// <summary>
    /// Fører bruker gjennom innlogging og åpner riktig meny basert på rolle.
    /// </summary>
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

    /// <summary>
    /// Lagrer hele applikasjonstilstanden til JSON-fil når bruker avslutter.
    /// </summary>
    private void LagreDataTilFil()
    {
        var snapshot = new AppStateSnapshot(
            _authFlow.Studenter.ToList(),
            _authFlow.Ansatte.ToList(),
            _kursManager.HentAlleKurs().ToList(),
            _bibliotekManager.HentAlleBøker().ToList(),
            _bibliotekManager.HentHistorikk(),
            _authService.Accounts.ToList());

        var lagreResult = AppStateStore.Lagre(_dataFilsti, snapshot);
        if (!lagreResult.IsSuccess)
        {
            Console.WriteLine($"Advarsel: {lagreResult.Message}");
        }
    }

    /// <summary>
    /// Finner data.json-filen i gjeldende mappe eller brukerens hjemmemappe.
    /// </summary>
    /// <returns>Full filsti til data.json</returns>
    private static string FinnDataFilsti()
    {
        string nåværendeMappe = Directory.GetCurrentDirectory();
        string filINåværendeMappe = Path.Combine(nåværendeMappe, "data.json");
        if (File.Exists(filINåværendeMappe))
        {
            return filINåværendeMappe;
        }

        string filIProsjektMappe = Path.Combine(nåværendeMappe, "UniversitetApp", "data.json");
        if (File.Exists(filIProsjektMappe) || Directory.Exists(Path.Combine(nåværendeMappe, "UniversitetApp")))
        {
            return filIProsjektMappe;
        }

        return filINåværendeMappe;
    }
}
