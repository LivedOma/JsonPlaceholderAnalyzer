using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Application.Interfaces;

/// <summary>
/// Interfaz base para servicios de entidades.
/// Define las operaciones comunes que todos los servicios deben implementar.
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IEntityService<T> where T : class
{
    Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> GetCountAsync(CancellationToken cancellationToken = default);
    Task<Result<T>> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}