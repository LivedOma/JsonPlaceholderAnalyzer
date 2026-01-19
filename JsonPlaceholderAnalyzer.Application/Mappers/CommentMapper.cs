using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class CommentMapper : IMapper<ApiCommentDto, Comment>
{
    public Comment Map(ApiCommentDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new Comment
        {
            Id = source.Id,
            PostId = source.PostId,
            Name = source.Name,
            Email = source.Email,
            Body = source.Body
        };
    }
}