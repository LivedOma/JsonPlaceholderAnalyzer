using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz para el cliente HTTP que consume la API.
/// </summary>
public interface IApiClient
{
    Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<T>>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<Result<T>> PostAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default);
    Task<Result<T>> PutAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}