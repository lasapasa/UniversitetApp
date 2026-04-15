using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp;

public static class SeedDataInitializer
{
    public static void Initialize(
        KursManager km,
        BibliotekManager bm,
        AuthService auth,
        out List<Student> studenter,
        out List<Ansatt> ansatte)
    {
        studenter = new List<Student>
        {
            new Student("S001", "Kari Nordmann", "kari@uni.no"),
            new Student("S002", "Ola Hansen", "ola@uni.no"),
            new Student("S004", "Lina Aas", "lina@uni.no"),
            new Student("S005", "Jonas Berg", "jonas@uni.no"),
            new Utvekslingsstudent(
                "S003", "Emma Schmidt", "emma@berlin.de",
                "Humboldt-Universität", "Tyskland",
                new DateOnly(2025, 8, 1), new DateOnly(2026, 6, 30))
        };

        ansatte = new List<Ansatt>
        {
            new Ansatt("A001", "Prof. Larsen", "larsen@uni.no", StillingType.Foreleser, "Informatikk"),
            new Ansatt("A002", "Maja Bakke", "maja@uni.no", StillingType.Bibliotekar, "Bibliotek")
        };

        km.OpprettKurs("INF101", "Programmering 1", 10, 30, "A001");
        km.OpprettKurs("MAT201", "Lineær Algebra", 10, 25, "A001");
        km.OpprettKurs("INF201", "Objektorientert programmering", 10, 20, "A001");

        km.MeldPåKurs(studenter[0], "INF101");
        km.MeldPåKurs(studenter[1], "INF101");
        km.MeldPåKurs(studenter[2], "MAT201");

        bm.RegistrerBok("B001", "Clean Code", "Robert C. Martin", 2008, 3);
        bm.RegistrerBok("B002", "The Pragmatic Programmer", "Hunt & Thomas", 1999, 2);
        bm.RegistrerBok("B003", "C# in Depth", "Jon Skeet", 2019, 4);

        bm.LånUtBok(studenter[0], "B001");
        bm.LånUtBok(ansatte[0], "B003");

        auth.Register("kari", "pass123", AppRole.Student, "S001");
        auth.Register("ola", "pass123", AppRole.Student, "S002");
        auth.Register("larsen", "pass123", AppRole.Faglærer, "A001");
        auth.Register("maja", "pass123", AppRole.BibliotekAnsatt, "A002");

        Console.WriteLine("[Eksempeldata og brukerkontoer lastet inn]");
        Console.WriteLine("Eksempel-innlogging: kari/pass123, larsen/pass123, maja/pass123");
    }
}
