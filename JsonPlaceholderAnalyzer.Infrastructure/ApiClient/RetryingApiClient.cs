using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Interfaces;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient;

/// <summary>
/// Decorador que agrega lógica de reintentos al cliente de API.
/// Demuestra: Patrón Decorator, Primary Constructor, async/await avanzado,
/// operador ??= (null-coalescing assignment).
/// </summary>
public class RetryingApiClient(IApiClient innerClient, ApiSettings settings) : IApiClient
{
    // Campo que puede ser asignado una sola vez con ??=
    private int? _cachedMaxRetries;
    
    // Propiedad que usa ??= para lazy initialization
    private int MaxRetries => _cachedMaxRetries ??= settings.MaxRetries;

    public async Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            () => innerClient.GetAsync<T>(endpoint, cancellationToken),
            cancellationToken
        );
    }

    public async Task<Result<IEnumerable<T>>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(
            () => innerClient.GetListAsync<T>(endpoint, cancellationToken),
            cancellationToken
        );
    }

    public async Task<Result<T>> PostAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
    {
        // POST generalmente no se reintenta para evitar duplicados
        return await innerClient.PostAsync(endpoint, data, cancellationToken);
    }

    public async Task<Result<T>> PutAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
    {
        // PUT es idempotente, se puede reintentar
        return await ExecuteWithRetryAsync(
            () => innerClient.PutAsync(endpoint, data, cancellationToken),
            cancellationToken
        );
    }

    public async Task<Result> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        // DELETE es idempotente, se puede reintentar
        return await ExecuteWithRetryAsync(
            () => innerClient.DeleteAsync(endpoint, cancellationToken),
            cancellationToken
        );
    }

    /// <summary>
    /// Ejecuta una operación con reintentos usando backoff exponencial.
    /// Demuestra: Func<Task<T>>, async/await avanzado, cálculo de delays.
    /// </summary>
    private async Task<Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        Result<T>? lastResult = null;
        
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                // Backoff exponencial: delay * 2^(attempt-1)
                var delay = settings.RetryDelayMilliseconds * (int)Math.Pow(2, attempt - 1);
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ↻ Retry attempt {attempt}/{MaxRetries} after {delay}ms...");
                Console.ResetColor();
                
                await Task.Delay(delay, cancellationToken);
            }
            
            lastResult = await operation();
            
            // Si fue exitoso o el error no es recuperable, salir
            if (lastResult.IsSuccess || !IsRetryableError(lastResult.Error))
            {
                return lastResult;
            }
        }
        
        // Operador ?? para asegurar que nunca retornamos null
        return lastResult ?? Result<T>.Failure("Unknown error after retries");
    }

    /// <summary>
    /// Versión para operaciones sin valor de retorno.
    /// </summary>
    private async Task<Result> ExecuteWithRetryAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken)
    {
        Result? lastResult = null;
        
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = settings.RetryDelayMilliseconds * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delay, cancellationToken);
            }
            
            lastResult = await operation();
            
            if (lastResult.IsSuccess || !IsRetryableError(lastResult.Error))
            {
                return lastResult;
            }
        }
        
        return lastResult ?? Result.Failure("Unknown error after retries");
    }

    /// <summary>
    /// Determina si un error es recuperable y vale la pena reintentar.
    /// Demuestra: pattern matching con strings, operador ?. null-conditional.
    /// </summary>
    private static bool IsRetryableError(string? error)
    {
        if (error is null) return false;
        
        // Errores que vale la pena reintentar
        return error.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || error.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || error.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
            || error.Contains("429", StringComparison.OrdinalIgnoreCase)
            || error.Contains("503", StringComparison.OrdinalIgnoreCase)
            || error.Contains("502", StringComparison.OrdinalIgnoreCase);
    }
}