namespace UniversitetApp;

public static class InputHelper
{
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
