using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para gestionar álbumes.
/// </summary>
public class AlbumService(
    IAlbumRepository repository,
    NotificationService notificationService
) : EntityServiceBase<Album, IAlbumRepository>(repository, notificationService)
{
    protected override Result ValidateEntity(Album entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Title))
            return Result.Failure("Album title is required");
        
        if (entity.UserId <= 0)
            return Result.Failure("Valid user ID is required");
        
        return Result.Success();
    }

    /// <summary>
    /// Obtiene álbumes de un usuario.
    /// </summary>
    public async Task<Result<IEnumerable<Album>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<IEnumerable<Album>>.Failure("Invalid user ID");
        
        return await Repository.GetByUserIdAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Obtiene un álbum con sus fotos.
    /// </summary>
    public async Task<Result<Album>> GetWithPhotosAsync(
        int albumId, 
        CancellationToken cancellationToken = default)
    {
        if (albumId <= 0)
            return Result<Album>.Failure("Invalid album ID");
        
        NotificationService.OnNotification($"Loading album #{albumId} with photos...");
        
        return await Repository.GetWithPhotosAsync(albumId, cancellationToken);
    }

    /// <summary>
    /// Obtiene fotos de un álbum.
    /// </summary>
    public async Task<Result<IEnumerable<Photo>>> GetPhotosAsync(
        int albumId, 
        CancellationToken cancellationToken = default)
    {
        if (albumId <= 0)
            return Result<IEnumerable<Photo>>.Failure("Invalid album ID");
        
        return await Repository.GetPhotosForAlbumAsync(albumId, cancellationToken);
    }

    /// <summary>
    /// Obtiene estadísticas de álbumes.
    /// </summary>
    public async Task<Result<AlbumStatistics>> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var albumsResult = await GetAllAsync(cancellationToken);
        
        if (albumsResult.IsFailure)
            return Result<AlbumStatistics>.Failure(albumsResult.Error ?? "Failed to get albums");
        
        var albums = albumsResult.Value!.ToList();
        
        // Obtener conteo de fotos para el primer álbum como muestra
        var samplePhotosResult = await GetPhotosAsync(1, cancellationToken);
        var samplePhotoCount = samplePhotosResult.IsSuccess 
            ? samplePhotosResult.Value?.Count() ?? 0 
            : 0;
        
        var stats = new AlbumStatistics
        {
            TotalAlbums = albums.Count,
            AlbumsPerUser = albums.GroupBy(a => a.UserId).ToDictionary(g => g.Key, g => g.Count()),
            EstimatedTotalPhotos = samplePhotoCount * albums.Count, // Estimación
            AverageAlbumsPerUser = albums.Count > 0 
                ? (double)albums.Count / albums.Select(a => a.UserId).Distinct().Count() 
                : 0
        };
        
        return Result<AlbumStatistics>.Success(stats);
    }
}

/// <summary>
/// DTO para estadísticas de álbumes.
/// </summary>
public record AlbumStatistics
{
    public int TotalAlbums { get; init; }
    public int EstimatedTotalPhotos { get; init; }
    public double AverageAlbumsPerUser { get; init; }
    public Dictionary<int, int> AlbumsPerUser { get; init; } = new();
}