namespace JsonPlaceholderAnalyzer.Infrastructure.Configuration;

/// <summary>
/// Configuración para el cliente de API.
/// Demuestra: required members, init-only properties, valores por defecto.
/// </summary>
public class ApiSettings
{
    public required string BaseUrl { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 1000;
    
    /// <summary>
    /// Configuración por defecto para JSONPlaceholder.
    /// </summary>
    public static ApiSettings Default => new()
    {
        BaseUrl = "https://jsonplaceholder.typicode.com",
        TimeoutSeconds = 30,
        MaxRetries = 3,
        RetryDelayMilliseconds = 1000
    };
}