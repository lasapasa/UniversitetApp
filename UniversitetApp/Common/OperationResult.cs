namespace UniversitetApp;

/// <summary>
/// Innkapsler resultatet av en operasjon (suksess eller feil).
/// Verwendet for konsistent feilhåndtering og meldinger.
/// </summary>
public class OperationResult
{
    /// <summary>Om operasjonen var vellykket</summary>
    public bool IsSuccess { get; }
    
    /// <summary>Statusmelding (suksess- eller feilmelding)</summary>
    public string Message { get; }
    
    /// <summary>Maskinlesbar feilkode for spesifikk feilhåndtering</summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initialiserer et resultat av operasjon.
    /// </summary>
    /// <param name="isSuccess">Om operasjonen var vellykket</param>
    /// <param name="message">Statusmelding</param>
    /// <param name="errorCode">Valgfri feilkode for å identifisere spesifikk feil</param>
    protected OperationResult(bool isSuccess, string message, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    /// <summary>Oppretter et vellykket resultat</summary>
    /// <param name="message">Suksessmelding</param>
    public static OperationResult Success(string message) => new(true, message);
    
    /// <summary>Oppretter et mislykket resultat</summary>
    /// <param name="message">Feilmelding</param>
    /// <param name="errorCode">Valgfri feilkode</param>
    public static OperationResult Failure(string message, string? errorCode = null) => new(false, message, errorCode);
}

/// <summary>
/// Innkapsler resultatet av en operasjon som returnerer data.
/// </summary>
/// <typeparam name="T">Type av data som returneres ved suksess</typeparam>
public class OperationResult<T> : OperationResult
{
    /// <summary>Dataene som returneres (null hvis operasjon mislyktes)</summary>
    public T? Data { get; }

    /// <summary>
    /// Initialiserer et resultat av operasjon med data.
    /// </summary>
    private OperationResult(bool isSuccess, string message, T? data = default, string? errorCode = null)
        : base(isSuccess, message, errorCode)
    {
        Data = data;
    }

    /// <summary>Oppretter et vellykket resultat med data</summary>
    /// <param name="message">Suksessmelding</param>
    /// <param name="data">Dataene som skal returneres</param>
    public static OperationResult<T> Success(string message, T data) => new(true, message, data);
    
    /// <summary>Oppretter et mislykket resultat</summary>
    /// <param name="message">Feilmelding</param>
    /// <param name="errorCode">Valgfri feilkode</param>
    public static new OperationResult<T> Failure(string message, string? errorCode = null) => new(false, message, default, errorCode);
}
