using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

/// <summary>
/// Meny for faglærer-operasjoner.
/// Tillater kursopprettelse, karaktersetting, pensum-registrering og bibliotekoperasjoner.
/// </summary>
public class FaglaererMenu
{
    private readonly KursManager _kursManager;
    private readonly BibliotekManager _bibliotekManager;
    private readonly List<Student> _studenter;
    private readonly Dictionary<string, Student> _studenterById = new(StringComparer.OrdinalIgnoreCase);
    private int _studentAntallVedSisteIndeks;

    /// <summary>
    /// Initialiserer en faglærermeny.
    /// </summary>
    /// <param name="kursManager">Manager for kursoperasjoner</param>
    /// <param name="bibliotekManager">Manager for bibliotekoperasjoner</param>
    /// <param name="studenter">Referanse til alle systemets studenter</param>
    public FaglaererMenu(KursManager kursManager, BibliotekManager bibliotekManager, List<Student> studenter)
    {
        _kursManager = kursManager;
        _bibliotekManager = bibliotekManager;
        _studenter = studenter;
        OppdaterStudentIndeksHvisNødvendig();
    }

    public void Run(Ansatt lærer)
    {
        bool aktiv = true;
        while (aktiv)
        {
            Console.WriteLine($"\n=== Faglærermeny ({lærer.Navn}) ===");
            Console.WriteLine("[1] Opprett kurs");
            Console.WriteLine("[2] Søk kurs");
            Console.WriteLine("[3] Søk bok");
            Console.WriteLine("[4] Lån bok");
            Console.WriteLine("[5] Returner bok");
            Console.WriteLine("[6] Sett karakter");
            Console.WriteLine("[7] Registrer pensum");
            Console.WriteLine("[8] Se kurs jeg underviser");
            Console.WriteLine("[0] Logg ut");
            string valg = InputHelper.LesIkkeTom("Velg: ");

            try
            {
                switch (valg)
                {
                    case "1":
                    {
                        string kode = InputHelper.LesIkkeTom("Kurskode: ");
                        string navn = InputHelper.LesIkkeTom("Kursnavn: ");
                        int stp = InputHelper.LesInt("Studiepoeng: ");
                        int maks = InputHelper.LesInt("Maks plasser: ");
                        var result = _kursManager.OpprettKurs(kode, navn, stp, maks, lærer.AnsattID);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "2":
                    {
                        string søk = InputHelper.LesIkkeTom("Søk (kode/navn): ");
                        var treff = _kursManager.FinnKursEtterSøk(søk);
                        if (treff.Count == 0) Console.WriteLine("Ingen kurs funnet.");
                        else foreach (var k in treff) Console.WriteLine($"- {k}");
                        break;
                    }
                    case "3":
                        VisBoktreff(InputHelper.LesIkkeTom("Søk (tittel, forfatter, ID): "));
                        break;
                    case "4":
                    {
                        string bokId = InputHelper.LesIkkeTom("Bok-ID: ");
                        var result = _bibliotekManager.LånUtBok(lærer, bokId);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "5":
                    {
                        string bokId = InputHelper.LesIkkeTom("Bok-ID: ");
                        var result = _bibliotekManager.ReturnerBok(lærer, bokId);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "6":
                    {
                        string kursKode = InputHelper.LesIkkeTom("Kurskode: ");
                        string studentID = InputHelper.LesIkkeTom("StudentID: ");
                        OppdaterStudentIndeksHvisNødvendig();
                        if (!_studenterById.ContainsKey(studentID.Trim()))
                        {
                            Console.WriteLine("Feil: Student finnes ikke.");
                            break;
                        }

                        string karakter = InputHelper.LesIkkeTom("Karakter (A-F): ");
                        var result = _kursManager.SettKarakter(lærer, kursKode, studentID, karakter);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "7":
                    {
                        string kursKode = InputHelper.LesIkkeTom("Kurskode: ");
                        string bokID = InputHelper.LesIkkeTom("Pensum bok-ID: ");
                        var result = _kursManager.RegistrerPensum(lærer, kursKode, bokID);
                        Console.WriteLine(result.IsSuccess ? result.Message : $"Feil: {result.Message}");
                        break;
                    }
                    case "8":
                    {
                        var kurs = _kursManager.HentKursForLærer(lærer.AnsattID);
                        if (kurs.Count == 0)
                        {
                            Console.WriteLine("Ingen kurs registrert på deg.");
                        }
                        else
                        {
                            foreach (var k in kurs)
                            {
                                Console.WriteLine($"- {k}");
                                if (k.PensumBokIDs.Count > 0)
                                    Console.WriteLine($"  Pensum: {string.Join(", ", k.PensumBokIDs)}");
                            }
                        }

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
                Console.WriteLine($"Feil i faglærermeny: {ex.Message}");
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

    private void OppdaterStudentIndeksHvisNødvendig()
    {
        if (_studentAntallVedSisteIndeks == _studenter.Count)
        {
            return;
        }

        _studenterById.Clear();
        foreach (var student in _studenter)
        {
            _studenterById[student.StudentID] = student;
        }

        _studentAntallVedSisteIndeks = _studenter.Count;
    }
}
