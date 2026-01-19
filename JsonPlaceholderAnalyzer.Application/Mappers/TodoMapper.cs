using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class TodoMapper : IMapper<ApiTodoDto, Todo>
{
    public Todo Map(ApiTodoDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new Todo
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title,
            Completed = source.Completed
        };
    }
}