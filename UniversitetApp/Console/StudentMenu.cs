using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

/// <summary>
/// Meny for studentbrukerens interaktive operasjoner.
/// Tillater påmelding/avmelding fra kurs, visning av karakterer og bibliotekoperasjoner.
/// </summary>
public class StudentMenu
{
    private readonly KursManager _kursManager;
    private readonly BibliotekManager _bibliotekManager;

    /// <summary>
    /// Initialiserer en studentmeny.
    /// </summary>
    /// <param name="kursManager">Manager for kursoperasjoner</param>
    /// <param name="bibliotekManager">Manager for bøkoperasjoner</param>
    public StudentMenu(KursManager kursManager, BibliotekManager bibliotekManager)
    {
        _kursManager = kursManager;
        _bibliotekManager = bibliotekManager;
    }

    /// <summary>
    /// Kjører studentmeny-løkka for en spesifikk student.
    /// </summary>
    /// <param name="student">Studenten som er logget inn</param>
    public void Run(Student student)
    {
        bool aktiv = true;
        while (aktiv)
        {
            Console.WriteLine($"\n=== Studentmeny ({student.Navn}) ===");
            Console.WriteLine("[1] Meld på kurs");
            Console.WriteLine("[2] Meld av kurs");
            Console.WriteLine("[3] Se mine kurs");
            Console.WriteLine("[4] Se karakterer");
            Console.WriteLine("[5] Søk bok");
            Console.WriteLine("[6] Lån bok");
            Console.WriteLine("[7] Returner bok");
            Console.WriteLine("[0] Logg ut");
            string valg = InputHelper.LesIkkeTom("Velg: ");

            try
            {
                switch (valg)
                {
                    case "1":
                    {
                        string kode = InputHelper.LesIkkeTom("Kurskode: ");
                        var result = _kursManager.MeldPåKurs(student, kode);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "2":
                    {
                        string kode = InputHelper.LesIkkeTom("Kurskode: ");
                        var result = _kursManager.MeldAvKurs(student, kode);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "3":
                    {
                        var mine = _kursManager.HentKursForStudent(student.StudentID);
                        if (mine.Count == 0)
                        {
                            Console.WriteLine("Ingen kurs registrert.");
                        }
                        else
                        {
                            foreach (var k in mine) Console.WriteLine($"- {k}");
                        }
                        break;
                    }
                    case "4":
                    {
                        var mine = _kursManager.HentKursForStudent(student.StudentID);
                        if (mine.Count == 0)
                        {
                            Console.WriteLine("Ingen kurs registrert.");
                        }
                        else
                        {
                            foreach (var k in mine)
                                Console.WriteLine($"- {k.KursKode}: {_kursManager.HentKarakter(student, k.KursKode)}");
                        }
                        break;
                    }
                    case "5":
                        VisBoktreff(InputHelper.LesIkkeTom("Søk (tittel, forfatter, ID): "));
                        break;
                    case "6":
                    {
                        string bokId = InputHelper.LesIkkeTom("Bok-ID: ");
                        var result = _bibliotekManager.LånUtBok(student, bokId);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "7":
                    {
                        string bokId = InputHelper.LesIkkeTom("Bok-ID: ");
                        var result = _bibliotekManager.ReturnerBok(student, bokId);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
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
                Console.WriteLine($"Feil i studentmeny: {ex.Message}");
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
