using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class PhotoMapper : IMapper<ApiPhotoDto, Photo>
{
    public Photo Map(ApiPhotoDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new Photo
        {
            Id = source.Id,
            AlbumId = source.AlbumId,
            Title = source.Title,
            Url = source.Url,
            ThumbnailUrl = source.ThumbnailUrl
        };
    }
}