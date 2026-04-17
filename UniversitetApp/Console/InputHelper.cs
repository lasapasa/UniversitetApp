namespace UniversitetApp;

/// <summary>
/// Utility-klasse for konsol-input-validering.
/// Sikrer at brukeren gir gyldig input for ulike datatyper.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Les en ikke-tom tekststreng fra konsolen.
    /// Gjentar spørsmål hvis bruker gir tomt input.
    /// </summary>
    /// <param name="prompt">Meldingne som vises til brukeren</param>
    /// <returns>Ikke-tom tekststreng trimmet for whitespace</returns>
    public static string LesIkkeTom(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string verdi = (Console.ReadLine() ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(verdi)) return verdi;
            Console.WriteLine("Input kan ikke være tom.");
        }
    }

    /// <summary>
    /// Les et heltall fra konsolen.
    /// Gjentar spørsmål hvis bruker gir ugyldig heltall.
    /// </summary>
    /// <param name="prompt">Meldingen som vises til brukeren</param>
    /// <returns>Gyldig heltall</returns>
    public static int LesInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse((Console.ReadLine() ?? string.Empty).Trim(), out int verdi))
                return verdi;
            Console.WriteLine("Skriv inn et gyldig heltall.");
        }
    }
}
