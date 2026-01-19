using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Repositorio específico para álbumes con operaciones adicionales.
/// </summary>
public interface IAlbumRepository : IRepository<Album>
{
    Task<Result<IEnumerable<Album>>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<Album>> GetWithPhotosAsync(int albumId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Photo>>> GetPhotosForAlbumAsync(int albumId, CancellationToken cancellationToken = default);
}