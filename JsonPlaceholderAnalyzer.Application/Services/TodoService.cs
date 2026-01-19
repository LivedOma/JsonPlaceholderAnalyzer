using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Enums;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para gestionar tareas (todos).
/// </summary>
public class TodoService(
    ITodoRepository repository,
    NotificationService notificationService
) : EntityServiceBase<Todo, ITodoRepository>(repository, notificationService)
{
    protected override Result ValidateEntity(Todo entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Title))
            return Result.Failure("Todo title is required");
        
        if (entity.Title.Length > 200)
            return Result.Failure("Todo title cannot exceed 200 characters");
        
        if (entity.UserId <= 0)
            return Result.Failure("Valid user ID is required");
        
        return Result.Success();
    }

    /// <summary>
    /// Obtiene todos de un usuario.
    /// </summary>
    public async Task<Result<IEnumerable<Todo>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<IEnumerable<Todo>>.Failure("Invalid user ID");
        
        return await Repository.GetByUserIdAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Obtiene todos completados.
    /// </summary>
    public async Task<Result<IEnumerable<Todo>>> GetCompletedAsync(
        CancellationToken cancellationToken = default)
    {
        return await Repository.GetCompletedAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene todos pendientes.
    /// </summary>
    public async Task<Result<IEnumerable<Todo>>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await Repository.GetPendingAsync(cancellationToken);
    }

    /// <summary>
    /// Cambia el estado de completado de un todo.
    /// </summary>
    public async Task<Result<Todo>> ToggleCompletedAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result<Todo>.Failure("Invalid todo ID");
        
        NotificationService.OnNotification($"Toggling todo #{id} completion status");
        
        return await Repository.ToggleCompletedAsync(id, cancellationToken);
    }

    /// <summary>
    /// Marca un todo como completado.
    /// </summary>
    public async Task<Result<Todo>> MarkAsCompletedAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var todoResult = await GetByIdAsync(id, cancellationToken);
        
        if (todoResult.IsFailure)
            return todoResult;
        
        if (todoResult.Value!.Completed)
            return todoResult; // Ya está completado
        
        return await ToggleCompletedAsync(id, cancellationToken);
    }

    /// <summary>
    /// Marca un todo como pendiente.
    /// </summary>
    public async Task<Result<Todo>> MarkAsPendingAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var todoResult = await GetByIdAsync(id, cancellationToken);
        
        if (todoResult.IsFailure)
            return todoResult;
        
        if (!todoResult.Value!.Completed)
            return todoResult; // Ya está pendiente
        
        return await ToggleCompletedAsync(id, cancellationToken);
    }

    /// <summary>
    /// Obtiene todos filtrados por prioridad.
    /// </summary>
    public async Task<Result<IEnumerable<Todo>>> GetByPriorityAsync(
        TodoPriority priority, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return allResult;
        
        var filtered = allResult.Value!.Where(t => t.Priority == priority);
        
        return Result<IEnumerable<Todo>>.Success(filtered);
    }

    /// <summary>
    /// Obtiene estadísticas de todos.
    /// </summary>
    public async Task<Result<TodoStatistics>> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<TodoStatistics>.Failure(allResult.Error ?? "Failed to get todos");
        
        var todos = allResult.Value!.ToList();
        
        var stats = new TodoStatistics
        {
            Total = todos.Count,
            Completed = todos.Count(t => t.Completed),
            Pending = todos.Count(t => !t.Completed),
            HighPriority = todos.Count(t => t.Priority == TodoPriority.High),
            MediumPriority = todos.Count(t => t.Priority == TodoPriority.Medium),
            LowPriority = todos.Count(t => t.Priority == TodoPriority.Low),
            CompletionRate = todos.Count > 0 
                ? (double)todos.Count(t => t.Completed) / todos.Count * 100 
                : 0,
            TodosPerUser = todos.GroupBy(t => t.UserId)
                .ToDictionary(g => g.Key, g => new UserTodoStats
                {
                    Total = g.Count(),
                    Completed = g.Count(t => t.Completed),
                    Pending = g.Count(t => !t.Completed)
                })
        };
        
        return Result<TodoStatistics>.Success(stats);
    }
}

/// <summary>
/// DTO para estadísticas de todos.
/// </summary>
public record TodoStatistics
{
    public int Total { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public int HighPriority { get; init; }
    public int MediumPriority { get; init; }
    public int LowPriority { get; init; }
    public double CompletionRate { get; init; }
    public Dictionary<int, UserTodoStats> TodosPerUser { get; init; } = new();
}

/// <summary>
/// DTO para estadísticas de todos por usuario.
/// </summary>
public record UserTodoStats
{
    public int Total { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public double CompletionRate => Total > 0 ? (double)Completed / Total * 100 : 0;
}