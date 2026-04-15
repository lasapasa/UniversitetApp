using UniversitetApp.Models;
using UniversitetApp.Services;

namespace UniversitetApp.Tests;

public class UnitTest1
{
    [Fact]
    public void OpprettKurs_MedDuplikatKodeEllerNavn_SkalFeile()
    {
        var km = new KursManager();

        var ok1 = km.OpprettKurs("INF101", "Programmering 1", 10, 30, "A001");
        var ok2 = km.OpprettKurs("INF101", "Nytt Navn", 10, 30, "A001");
        var ok3 = km.OpprettKurs("INF102", "Programmering 1", 10, 30, "A001");

        Assert.True(ok1.IsSuccess);
        Assert.False(ok2.IsSuccess);
        Assert.False(ok3.IsSuccess);
    }

    [Fact]
    public void MeldPåKurs_SammeStudentToGanger_SkalFeileAndreGang()
    {
        var km = new KursManager();
        var student = new Student("S001", "Kari", "kari@uni.no");
        km.OpprettKurs("INF201", "Objektorientert", 10, 30, "A001");

        var ok1 = km.MeldPåKurs(student, "INF201");
        var ok2 = km.MeldPåKurs(student, "INF201");

        Assert.True(ok1.IsSuccess);
        Assert.False(ok2.IsSuccess);
        Assert.Single(student.KursKoder);
    }

    [Fact]
    public void SettKarakter_ForKursLærerIkkeUnderviser_SkalFeile()
    {
        var km = new KursManager();
        var riktigLærer = new Ansatt("A001", "Lars", "lars@uni.no", StillingType.Foreleser, "INF");
        var annenLærer = new Ansatt("A002", "Maja", "maja@uni.no", StillingType.Foreleser, "INF");
        var student = new Student("S001", "Kari", "kari@uni.no");

        km.OpprettKurs("INF301", "Databaser", 10, 30, riktigLærer.AnsattID);
        km.MeldPåKurs(student, "INF301");

        var ok = km.SettKarakter(annenLærer, "INF301", student.StudentID, "A");

        Assert.False(ok.IsSuccess);
        Assert.Equal("Ikke satt", km.HentKarakter(student, "INF301"));
    }

    [Fact]
    public void ReturnerBok_AktivtLån_SkalØkeTilgjengeligeEksemplarer()
    {
        var bm = new BibliotekManager();
        var student = new Student("S001", "Kari", "kari@uni.no");

        bm.RegistrerBok("B001", "Clean Code", "Robert C. Martin", 2008, 2);

        var lånOk = bm.LånUtBok(student, "B001");
        var returOk = bm.ReturnerBok(student, "B001");

        var bok = bm.HentAlleBøker().Single(b => b.MediaID == "B001");

        Assert.True(lånOk.IsSuccess);
        Assert.True(returOk.IsSuccess);
        Assert.Equal(2, bok.TilgjengeligeEksemplarer);
    }
}
