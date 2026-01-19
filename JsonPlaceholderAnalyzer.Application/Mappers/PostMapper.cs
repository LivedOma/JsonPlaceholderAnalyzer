using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class PostMapper : IMapper<ApiPostDto, Post>
{
    public Post Map(ApiPostDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new Post
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title,
            Body = source.Body
        };
    }
}