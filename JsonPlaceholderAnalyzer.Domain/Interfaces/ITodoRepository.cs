using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Repositorio específico para todos con operaciones adicionales.
/// </summary>
public interface ITodoRepository : IRepository<Todo>
{
    Task<Result<IEnumerable<Todo>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Todo>>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Todo>>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<Result<Todo>> ToggleCompletedAsync(int id, CancellationToken cancellationToken = default);
}