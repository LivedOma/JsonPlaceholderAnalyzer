using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Repositories;

public class TodoRepository(
    IApiClient apiClient,
    IMapper<ApiTodoDto, Todo> mapper
) : RepositoryBase<Todo, ApiTodoDto>(apiClient, mapper, "todos"), ITodoRepository
{
    public async Task<Result<IEnumerable<Todo>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"todos?userId={userId}";
        var result = await ApiClient.GetListAsync<ApiTodoDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => 
                Result<IEnumerable<Todo>>.Success(result.Value.Select(dto => Mapper.Map(dto))),
            _ => Result<IEnumerable<Todo>>.Failure(result.Error ?? "Failed to get todos")
        };
    }

    public async Task<Result<IEnumerable<Todo>>> GetCompletedAsync(
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<IEnumerable<Todo>>.Failure(allResult.Error ?? "Failed to get todos");
        
        var completed = allResult.Value?
            .Where(t => t.Completed)
            .ToList() ?? [];
        
        return Result<IEnumerable<Todo>>.Success(completed);
    }

    public async Task<Result<IEnumerable<Todo>>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<IEnumerable<Todo>>.Failure(allResult.Error ?? "Failed to get todos");
        
        var pending = allResult.Value?
            .Where(t => !t.Completed)
            .ToList() ?? [];
        
        return Result<IEnumerable<Todo>>.Success(pending);
    }

    public async Task<Result<Todo>> ToggleCompletedAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync(id, cancellationToken);
        
        if (result.IsFailure)
            return Result<Todo>.Failure(result.Error ?? "Todo not found");
        
        var todo = result.Value!;
        
        var updatedTodo = new Todo
        {
            Id = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Completed = !todo.Completed
        };
        
        return await UpdateAsync(updatedTodo, cancellationToken);
    }
}