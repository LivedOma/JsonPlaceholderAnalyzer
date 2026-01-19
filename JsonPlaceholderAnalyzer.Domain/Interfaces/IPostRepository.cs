using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Repositorio específico para posts con operaciones adicionales.
/// </summary>
public interface IPostRepository : IRepository<Post>
{
    Task<Result<IEnumerable<Post>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Post>>> SearchByTitleAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Comment>>> GetCommentsForPostAsync(int postId, CancellationToken cancellationToken = default);
}