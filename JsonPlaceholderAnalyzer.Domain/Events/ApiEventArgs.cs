namespace JsonPlaceholderAnalyzer.Domain.Events;

/// <summary>
/// Argumentos para eventos de llamadas a la API.
/// </summary>
public class ApiRequestEventArgs : EventArgs
{
    public string Endpoint { get; }
    public string HttpMethod { get; }
    public DateTime Timestamp { get; }

    public ApiRequestEventArgs(string endpoint, string httpMethod)
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        HttpMethod = httpMethod ?? throw new ArgumentNullException(nameof(httpMethod));
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos para eventos de respuesta de la API.
/// </summary>
public class ApiResponseEventArgs : EventArgs
{
    public string Endpoint { get; }
    public bool IsSuccess { get; }
    public int? StatusCode { get; }
    public TimeSpan Duration { get; }
    public string? ErrorMessage { get; }
    public DateTime Timestamp { get; }

    public ApiResponseEventArgs(
        string endpoint, 
        bool isSuccess, 
        TimeSpan duration,
        int? statusCode = null,
        string? errorMessage = null)
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Duration = duration;
        ErrorMessage = errorMessage;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos para eventos de error de la API.
/// </summary>
public class ApiErrorEventArgs : EventArgs
{
    public string Endpoint { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public DateTime Timestamp { get; }

    public ApiErrorEventArgs(string endpoint, string errorMessage, Exception? exception = null)
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        Exception = exception;
        Timestamp = DateTime.UtcNow;
    }
}