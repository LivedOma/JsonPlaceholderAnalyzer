using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Repositories;

public class AlbumRepository(
    IApiClient apiClient,
    IMapper<ApiAlbumDto, Album> mapper,
    IMapper<ApiPhotoDto, Photo> photoMapper
) : RepositoryBase<Album, ApiAlbumDto>(apiClient, mapper, "albums"), IAlbumRepository
{
    private readonly IMapper<ApiPhotoDto, Photo> _photoMapper = photoMapper;

    public async Task<Result<IEnumerable<Album>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"albums?userId={userId}";
        var result = await ApiClient.GetListAsync<ApiAlbumDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => 
                Result<IEnumerable<Album>>.Success(result.Value.Select(dto => Mapper.Map(dto))),
            _ => Result<IEnumerable<Album>>.Failure(result.Error ?? "Failed to get albums")
        };
    }

    public async Task<Result<Album>> GetWithPhotosAsync(
        int albumId, 
        CancellationToken cancellationToken = default)
    {
        var albumResult = await GetByIdAsync(albumId, cancellationToken);
        if (albumResult.IsFailure)
            return albumResult;
        
        var photosResult = await GetPhotosForAlbumAsync(albumId, cancellationToken);
        if (photosResult.IsFailure)
            return Result<Album>.Failure(photosResult.Error ?? "Failed to get photos");
        
        var albumWithPhotos = new Album
        {
            Id = albumResult.Value!.Id,
            UserId = albumResult.Value.UserId,
            Title = albumResult.Value.Title,
            Photos = photosResult.Value?.ToList() ?? []
        };
        
        return Result<Album>.Success(albumWithPhotos);
    }

    public async Task<Result<IEnumerable<Photo>>> GetPhotosForAlbumAsync(
        int albumId, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"albums/{albumId}/photos";
        var result = await ApiClient.GetListAsync<ApiPhotoDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => 
                Result<IEnumerable<Photo>>.Success(result.Value.Select(dto => _photoMapper.Map(dto))),
            _ => Result<IEnumerable<Photo>>.Failure(result.Error ?? "Failed to get photos")
        };
    }
}