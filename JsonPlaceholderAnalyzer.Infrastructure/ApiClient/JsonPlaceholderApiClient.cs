using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Interfaces;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient;

/// <summary>
/// Cliente HTTP para consumir la API de JSONPlaceholder.
/// Demuestra: Primary Constructor (C# 12), async/await, operadores modernos,
/// pattern matching, generics.
/// </summary>
public class JsonPlaceholderApiClient(HttpClient httpClient, ApiSettings settings) : IApiClient
{
    // Primary constructor - httpClient y settings son campos implícitos
    
    // Opciones de JSON compartidas (thread-safe)
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Obtiene un recurso individual de la API.
    /// Demuestra: async/await, generics, operadores ??, pattern matching.
    /// </summary>
    public async Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            // Operador ?? para asegurar que endpoint no sea null
            var url = BuildUrl(endpoint ?? throw new ArgumentNullException(nameof(endpoint)));
            
            // Await de la llamada HTTP
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            // Pattern matching con switch expression para manejar la respuesta
            return response.StatusCode switch
            {
                HttpStatusCode.OK => await DeserializeResponse<T>(response, cancellationToken),
                HttpStatusCode.NotFound => Result<T>.Failure($"Resource not found: {endpoint}"),
                HttpStatusCode.TooManyRequests => Result<T>.Failure("Rate limit exceeded. Please try again later."),
                _ => Result<T>.Failure($"API error: {response.StatusCode} - {response.ReasonPhrase}")
            };
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<T>.Failure("Request was cancelled.");
        }
        catch (TaskCanceledException)
        {
            return Result<T>.Failure("Request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure(ex);
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure($"Failed to deserialize response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    /// Obtiene una lista de recursos de la API.
    /// Demuestra: async/await con IEnumerable, operador ?. para null safety.
    /// </summary>
    public async Task<Result<IEnumerable<T>>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = BuildUrl(endpoint ?? throw new ArgumentNullException(nameof(endpoint)));
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<T>>.Failure($"API error: {response.StatusCode}");
            }
            
            // Deserializar como lista
            var content = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
            
            // Operador ?? para retornar lista vacía si es null
            return Result<IEnumerable<T>>.Success(content ?? []);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<T>>.Failure("Request was cancelled.");
        }
        catch (TaskCanceledException)
        {
            return Result<IEnumerable<T>>.Failure("Request timed out.");
        }
        catch (HttpRequestException ex)
        {
            return Result<IEnumerable<T>>.Failure(ex);
        }
        catch (JsonException ex)
        {
            return Result<IEnumerable<T>>.Failure($"Failed to deserialize response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<T>>.Failure(ex);
        }
    }

    /// <summary>
    /// Crea un nuevo recurso en la API.
    /// Nota: JSONPlaceholder simula la creación pero no persiste datos.
    /// </summary>
    public async Task<Result<T>> PostAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
    {
        try
        {
            // Operador ?? para validar
            ArgumentNullException.ThrowIfNull(data);
            
            var url = BuildUrl(endpoint ?? throw new ArgumentNullException(nameof(endpoint)));
            
            var response = await httpClient.PostAsJsonAsync(url, data, JsonOptions, cancellationToken);
            
            return response.IsSuccessStatusCode
                ? await DeserializeResponse<T>(response, cancellationToken)
                : Result<T>.Failure($"Failed to create resource: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    /// Actualiza un recurso existente en la API.
    /// Nota: JSONPlaceholder simula la actualización pero no persiste datos.
    /// </summary>
    public async Task<Result<T>> PutAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(data);
            
            var url = BuildUrl(endpoint ?? throw new ArgumentNullException(nameof(endpoint)));
            
            var response = await httpClient.PutAsJsonAsync(url, data, JsonOptions, cancellationToken);
            
            return response.IsSuccessStatusCode
                ? await DeserializeResponse<T>(response, cancellationToken)
                : Result<T>.Failure($"Failed to update resource: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    /// Elimina un recurso de la API.
    /// Nota: JSONPlaceholder simula la eliminación pero no persiste cambios.
    /// </summary>
    public async Task<Result> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = BuildUrl(endpoint ?? throw new ArgumentNullException(nameof(endpoint)));
            
            var response = await httpClient.DeleteAsync(url, cancellationToken);
            
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure($"Failed to delete resource: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }

    /// <summary>
    /// Construye la URL completa combinando base URL y endpoint.
    /// Demuestra: operador ?. y ?? para manejo de nulls.
    /// </summary>
    private string BuildUrl(string endpoint)
    {
        // Remover slash inicial si existe
        var cleanEndpoint = endpoint?.TrimStart('/') ?? string.Empty;
        
        // Asegurar que base URL no termine en slash
        var baseUrl = settings.BaseUrl?.TrimEnd('/') ?? throw new InvalidOperationException("BaseUrl is not configured");
        
        return $"{baseUrl}/{cleanEndpoint}";
    }

    /// <summary>
    /// Deserializa la respuesta HTTP a un tipo genérico.
    /// Demuestra: async/await, generics, operador ! (null-forgiving).
    /// </summary>
    private static async Task<Result<T>> DeserializeResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        
        // Operador ! indica que confiamos que content no es null después de éxito
        // En producción, agregaríamos validación adicional
        return content is not null
            ? Result<T>.Success(content)
            : Result<T>.Failure("Response content was null");
    }
}