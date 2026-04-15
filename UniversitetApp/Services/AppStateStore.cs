using System.Text.Json;
using UniversitetApp.Models;

namespace UniversitetApp.Services;

public sealed class AppStateSnapshot
{
    public List<Student> Studenter { get; }
    public List<Ansatt> Ansatte { get; }
    public List<Kurs> Kurs { get; }
    public List<Bok> Boker { get; }
    public List<Laan> LaanHistorikk { get; }
    public List<UserAccount> Accounts { get; }

    public AppStateSnapshot(
        List<Student> studenter,
        List<Ansatt> ansatte,
        List<Kurs> kurs,
        List<Bok> boker,
        List<Laan> laanHistorikk,
        List<UserAccount> accounts)
    {
        Studenter = studenter;
        Ansatte = ansatte;
        Kurs = kurs;
        Boker = boker;
        LaanHistorikk = laanHistorikk;
        Accounts = accounts;
    }
}

public static class AppStateStore
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static OperationResult<AppStateSnapshot> LastInn(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return OperationResult<AppStateSnapshot>.Failure("Ingen lagret tilstand funnet.", "state_not_found");
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var persisted = JsonSerializer.Deserialize<PersistedAppState>(json, ReadOptions);
            if (persisted == null)
            {
                return OperationResult<AppStateSnapshot>.Failure("Kunne ikke lese lagret tilstand.", "invalid_state");
            }
            var snapshot = AppStateMapper.HydrerSnapshot(persisted);
            return OperationResult<AppStateSnapshot>.Success("Lagret tilstand lastet.", snapshot);
        }
        catch (Exception ex)
        {
            return OperationResult<AppStateSnapshot>.Failure($"Kunne ikke laste data: {ex.Message}", "state_load_failed");
        }
    }

    public static OperationResult Lagre(string filePath, AppStateSnapshot snapshot)
    {
        try
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var persisted = AppStateMapper.ByggPersistedState(snapshot);
            string json = JsonSerializer.Serialize(persisted, WriteOptions);
            File.WriteAllText(filePath, json);

            return OperationResult.Success("Tilstand lagret.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Kunne ikke lagre data: {ex.Message}", "state_save_failed");
        }
    }
}
