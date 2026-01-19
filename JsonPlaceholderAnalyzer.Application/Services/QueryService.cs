using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para ejecutar consultas con filtrado, ordenamiento y paginación.
/// 
/// Demuestra:
/// - Pattern Matching para ordenamiento dinámico
/// - Switch expressions complejas
/// - Func<T, object> para ordenamiento genérico
/// </summary>
public class QueryService(
    UserService userService,
    PostService postService,
    TodoService todoService,
    ResponseMappingService mappingService,
    NotificationService notificationService)
{
    /// <summary>
    /// Consulta usuarios con filtrado, ordenamiento y paginación.
    /// </summary>
    public async Task<Result<PaginatedResponse<UserResponseDto>>> QueryUsersAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var usersResult = await userService.GetAllAsync(cancellationToken);
        
        if (usersResult.IsFailure)
            return Result<PaginatedResponse<UserResponseDto>>.Failure(usersResult.Error!);
        
        var users = usersResult.Value!.AsEnumerable();
        
        // Filtrar si hay término de búsqueda
        if (request.HasSearch)
        {
            users = FilterUsers(users, request.SearchTerm!);
        }
        
        // Ordenar si hay criterio de ordenamiento
        if (request.HasSort)
        {
            users = SortUsers(users, request.Sort);
        }
        
        // Mapear y paginar
        var dtos = mappingService.MapUsers(users).ToList();
        var paginatedResponse = PaginatedResponse<UserResponseDto>.Create(dtos, request.Pagination);
        
        notificationService.OnNotification(
            $"Query returned {paginatedResponse.ItemCount} of {paginatedResponse.TotalItems} users"
        );
        
        return Result<PaginatedResponse<UserResponseDto>>.Success(paginatedResponse);
    }

    /// <summary>
    /// Consulta posts con filtrado, ordenamiento y paginación.
    /// </summary>
    public async Task<Result<PaginatedResponse<PostResponseDto>>> QueryPostsAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var postsResult = await postService.GetAllAsync(cancellationToken);
        var usersResult = await userService.GetAllAsync(cancellationToken);
        
        if (postsResult.IsFailure)
            return Result<PaginatedResponse<PostResponseDto>>.Failure(postsResult.Error!);
        
        var posts = postsResult.Value!.AsEnumerable();
        var users = usersResult.IsSuccess ? usersResult.Value : null;
        
        // Filtrar
        if (request.HasSearch)
        {
            posts = FilterPosts(posts, request.SearchTerm!);
        }
        
        // Ordenar usando Pattern Matching
        if (request.HasSort)
        {
            posts = SortPosts(posts, request.Sort);
        }
        
        // Mapear y paginar
        var dtos = mappingService.MapPosts(posts, users).ToList();
        var paginatedResponse = PaginatedResponse<PostResponseDto>.Create(dtos, request.Pagination);
        
        return Result<PaginatedResponse<PostResponseDto>>.Success(paginatedResponse);
    }

    /// <summary>
    /// Consulta todos con filtros adicionales.
    /// </summary>
    public async Task<Result<PaginatedResponse<TodoResponseDto>>> QueryTodosAsync(
        QueryRequest request,
        bool? completedFilter = null,
        CancellationToken cancellationToken = default)
    {
        var todosResult = await todoService.GetAllAsync(cancellationToken);
        var usersResult = await userService.GetAllAsync(cancellationToken);
        
        if (todosResult.IsFailure)
            return Result<PaginatedResponse<TodoResponseDto>>.Failure(todosResult.Error!);
        
        var todos = todosResult.Value!.AsEnumerable();
        var userDict = usersResult.IsSuccess 
            ? usersResult.Value!.ToDictionary(u => u.Id) 
            : new Dictionary<int, User>();
        
        // Filtrar por estado de completado usando Pattern Matching
        todos = completedFilter switch
        {
            true => todos.Where(t => t.Completed),
            false => todos.Where(t => !t.Completed),
            null => todos
        };
        
        // Filtrar por búsqueda
        if (request.HasSearch)
        {
            todos = todos.Where(t => 
                t.Title.Contains(request.SearchTerm!, StringComparison.OrdinalIgnoreCase));
        }
        
        // Ordenar
        if (request.HasSort)
        {
            todos = SortTodos(todos, request.Sort);
        }
        
        // Mapear
        var dtos = todos.Select(t => 
        {
            var user = userDict.TryGetValue(t.UserId, out var u) ? u : null;
            return mappingService.MapTodo(t, user);
        }).ToList();
        
        var paginatedResponse = PaginatedResponse<TodoResponseDto>.Create(dtos, request.Pagination);
        
        return Result<PaginatedResponse<TodoResponseDto>>.Success(paginatedResponse);
    }

    #region Private Filter Methods

    /// <summary>
    /// Filtra usuarios usando Pattern Matching.
    /// </summary>
    private static IEnumerable<User> FilterUsers(IEnumerable<User> users, string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        
        return users.Where(user => 
            user.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            user.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            user.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            user.Company.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            user.Address.City.Contains(term, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Filtra posts.
    /// </summary>
    private static IEnumerable<Post> FilterPosts(IEnumerable<Post> posts, string searchTerm)
    {
        return posts.Where(post =>
            post.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            post.Body.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        );
    }

    #endregion

    #region Private Sort Methods with Pattern Matching

    /// <summary>
    /// Ordena usuarios usando Pattern Matching para selección dinámica de campo.
    /// Demuestra: Switch expression para seleccionar Func<T, object>.
    /// </summary>
    private static IEnumerable<User> SortUsers(IEnumerable<User> users, SortRequest sort)
    {
        // Pattern matching para seleccionar el campo de ordenamiento
        Func<User, object> keySelector = sort.SortBy?.ToLowerInvariant() switch
        {
            "name" => u => u.Name,
            "username" => u => u.Username,
            "email" => u => u.Email,
            "company" => u => u.Company.Name,
            "city" => u => u.Address.City,
            _ => u => u.Id // Default: ordenar por ID
        };
        
        return sort.Descending 
            ? users.OrderByDescending(keySelector) 
            : users.OrderBy(keySelector);
    }

    /// <summary>
    /// Ordena posts usando Pattern Matching.
    /// </summary>
    private static IEnumerable<Post> SortPosts(IEnumerable<Post> posts, SortRequest sort)
    {
        Func<Post, object> keySelector = sort.SortBy?.ToLowerInvariant() switch
        {
            "title" => p => p.Title,
            "userid" => p => p.UserId,
            "wordcount" or "words" => p => p.WordCount,
            "length" => p => p.Body.Length,
            _ => p => p.Id
        };
        
        return sort.Descending 
            ? posts.OrderByDescending(keySelector) 
            : posts.OrderBy(keySelector);
    }

    /// <summary>
    /// Ordena todos usando Pattern Matching.
    /// </summary>
    private static IEnumerable<Todo> SortTodos(IEnumerable<Todo> todos, SortRequest sort)
    {
        Func<Todo, object> keySelector = sort.SortBy?.ToLowerInvariant() switch
        {
            "title" => t => t.Title,
            "userid" => t => t.UserId,
            "completed" or "status" => t => t.Completed,
            "priority" => t => t.Priority,
            _ => t => t.Id
        };
        
        return sort.Descending 
            ? todos.OrderByDescending(keySelector) 
            : todos.OrderBy(keySelector);
    }

    #endregion
}