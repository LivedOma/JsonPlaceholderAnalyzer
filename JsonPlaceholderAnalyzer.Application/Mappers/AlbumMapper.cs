using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class AlbumMapper : IMapper<ApiAlbumDto, Album>
{
    public Album Map(ApiAlbumDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new Album
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title
        };
    }
}