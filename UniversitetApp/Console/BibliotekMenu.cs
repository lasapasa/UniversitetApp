using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

public class BibliotekMenu
{
    private readonly BibliotekManager _bibliotekManager;

    public BibliotekMenu(BibliotekManager bibliotekManager)
    {
        _bibliotekManager = bibliotekManager;
    }

    public void Run()
    {
        bool aktiv = true;
        while (aktiv)
        {
            Console.WriteLine("\n=== Bibliotekmeny ===");
            Console.WriteLine("[1] Registrer bok");
            Console.WriteLine("[2] Søk bok");
            Console.WriteLine("[3] Se aktive lån");
            Console.WriteLine("[4] Se lånehistorikk");
            Console.WriteLine("[0] Logg ut");

            string valg = InputHelper.LesIkkeTom("Velg: ");

            try
            {
                switch (valg)
                {
                    case "1":
                    {
                        string id = InputHelper.LesIkkeTom("Media-ID: ");
                        string tittel = InputHelper.LesIkkeTom("Tittel: ");
                        string forfatter = InputHelper.LesIkkeTom("Forfatter: ");
                        int aar = InputHelper.LesInt("Utgivelsesår: ");
                        int antall = InputHelper.LesInt("Antall eksemplarer: ");
                        var result = _bibliotekManager.RegistrerBok(id, tittel, forfatter, aar, antall);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "2":
                        VisBoktreff(InputHelper.LesIkkeTom("Søk (tittel, forfatter, ID): "));
                        break;
                    case "3":
                    {
                        var aktive = _bibliotekManager.HentAktiveLån();
                        if (aktive.Count == 0)
                        {
                            Console.WriteLine("Ingen aktive lån.");
                        }
                        else
                        {
                            Console.WriteLine($"\n{aktive.Count} aktivt lån:");
                            int nr = 1;
                            foreach (var l in aktive)
                            {
                                string brukerId = l.Bruker is Student st ? st.StudentID
                                                : l.Bruker is Ansatt ans ? ans.AnsattID
                                                : "ukjent";
                                Console.WriteLine($"{nr++}. [{brukerId}] {l.Bruker.Navn}");
                                Console.WriteLine($"   Bok: \"{l.Bok.Tittel}\" ({l.Bok.MediaID})");
                                Console.WriteLine($"   Lånt: {l.LånDato:dd.MM.yyyy}");
                            }
                        }
                        break;
                    }
                    case "4":
                    {
                        var historikk = _bibliotekManager.HentHistorikk();
                        if (historikk.Count == 0) Console.WriteLine("Ingen lånehistorikk.");
                        else foreach (var l in historikk) Console.WriteLine($"- {l}");
                        break;
                    }
                    case "0":
                        aktiv = false;
                        break;
                    default:
                        Console.WriteLine("Ugyldig valg.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Feil i bibliotekmeny: {ex.Message}");
            }
        }
    }

    private void VisBoktreff(string søkeord)
    {
        var treff = _bibliotekManager.FinnBøker(søkeord);
        if (treff.Count == 0)
        {
            Console.WriteLine("Ingen bøker funnet.");
            return;
        }

        foreach (var b in treff) Console.WriteLine($"- {b}");
    }
}
