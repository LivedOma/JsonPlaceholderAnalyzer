using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para mapear entidades de dominio a DTOs de respuesta.
/// 
/// Demuestra:
/// - Pattern Matching avanzado
/// - Switch expressions
/// - Type patterns
/// - Property patterns
/// - Relational patterns
/// </summary>
public class ResponseMappingService
{
    #region User Mapping

    /// <summary>
    /// Mapea un User a UserResponseDto.
    /// </summary>
    public UserResponseDto MapUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            City = user.Address.City,
            CompanyName = user.Company.Name,
            Phone = user.Phone,
            Website = user.Website
        };
    }

    /// <summary>
    /// Mapea una colección de usuarios.
    /// </summary>
    public IEnumerable<UserResponseDto> MapUsers(IEnumerable<User> users)
    {
        return users.Select(MapUser);
    }

    #endregion

    #region Post Mapping

    /// <summary>
    /// Mapea un Post a PostResponseDto, opcionalmente incluyendo info del autor.
    /// </summary>
    public PostResponseDto MapPost(Post post, User? author = null)
    {
        ArgumentNullException.ThrowIfNull(post);
        
        return new PostResponseDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Body = post.Body,
            AuthorName = author?.Name
        };
    }

    /// <summary>
    /// Mapea posts con pattern matching para determinar si incluir autor.
    /// Demuestra: Pattern matching con tuplas.
    /// </summary>
    public IEnumerable<PostResponseDto> MapPosts(
        IEnumerable<Post> posts, 
        IEnumerable<User>? users = null)
    {
        var userDict = users?.ToDictionary(u => u.Id) ?? new Dictionary<int, User>();
        
        return posts.Select(post =>
        {
            // Pattern matching con TryGetValue
            var author = userDict.TryGetValue(post.UserId, out var user) ? user : null;
            return MapPost(post, author);
        });
    }

    #endregion

    #region Todo Mapping

    /// <summary>
    /// Mapea un Todo a TodoResponseDto.
    /// </summary>
    public TodoResponseDto MapTodo(Todo todo, User? user = null)
    {
        ArgumentNullException.ThrowIfNull(todo);
        
        return new TodoResponseDto
        {
            Id = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Completed = todo.Completed,
            UserName = user?.Name
        };
    }

    #endregion

    #region Comment Mapping

    /// <summary>
    /// Mapea un Comment a CommentResponseDto.
    /// </summary>
    public CommentResponseDto MapComment(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        
        return new CommentResponseDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            Name = comment.Name,
            Email = comment.Email,
            Body = comment.Body
        };
    }

    #endregion

    #region Album & Photo Mapping

    /// <summary>
    /// Mapea un Album a AlbumResponseDto.
    /// </summary>
    public AlbumResponseDto MapAlbum(Album album, User? user = null)
    {
        ArgumentNullException.ThrowIfNull(album);
        
        return new AlbumResponseDto
        {
            Id = album.Id,
            UserId = album.UserId,
            Title = album.Title,
            PhotoCount = album.PhotoCount,
            UserName = user?.Name
        };
    }

    /// <summary>
    /// Mapea una Photo a PhotoResponseDto.
    /// </summary>
    public PhotoResponseDto MapPhoto(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        
        return new PhotoResponseDto
        {
            Id = photo.Id,
            AlbumId = photo.AlbumId,
            Title = photo.Title,
            Url = photo.Url,
            ThumbnailUrl = photo.ThumbnailUrl
        };
    }

    #endregion

    #region Pattern Matching Examples

    /// <summary>
    /// Clasifica una entidad usando pattern matching con tipos.
    /// Demuestra: Type patterns y switch expression.
    /// </summary>
    public string ClassifyEntity(object entity) => entity switch
    {
        User u => $"Usuario: {u.Name}",
        Post p => $"Post: {p.ShortTitle}",
        Comment c => $"Comentario: {c.ShortName}",
        Todo t => $"Tarea: {t.Title} ({t.Status})",
        Album a => $"Álbum: {a.Title}",
        Photo ph => $"Foto: {ph.Title}",
        null => "Entidad nula",
        _ => $"Tipo desconocido: {entity.GetType().Name}"
    };

    /// <summary>
    /// Obtiene prioridad de visualización usando property patterns.
    /// Demuestra: Property patterns anidados.
    /// </summary>
    public int GetDisplayPriority(object entity) => entity switch
    {
        // Property pattern: verifica propiedades del objeto
        User { Company.Name: "Romaguera-Crona" } => 1,  // Compañía específica primero
        User { Address.City: "Gwenborough" } => 2,       // Ciudad específica
        User => 3,                                        // Otros usuarios
        
        Post { WordCount: > 100 } => 1,                  // Posts largos primero
        Post { WordCount: > 50 } => 2,                   // Posts medianos
        Post => 3,                                        // Posts cortos
        
        Todo { Completed: false, Priority: Domain.Enums.TodoPriority.High } => 1,  // Urgente
        Todo { Completed: false } => 2,                  // Pendiente
        Todo { Completed: true } => 3,                   // Completado
        
        _ => 99
    };

    /// <summary>
    /// Formatea para consola usando relational patterns.
    /// Demuestra: Relational patterns (>, <, >=, <=).
    /// </summary>
    public string FormatForConsole(PostResponseDto post) => post.WordCount switch
    {
        // Relational patterns
        < 10 => $"📄 [Muy corto] {post.Title}",
        >= 10 and < 30 => $"📝 [Corto] {post.Title}",
        >= 30 and < 60 => $"📃 [Medio] {post.Title}",
        >= 60 and < 100 => $"📑 [Largo] {post.Title}",
        >= 100 => $"📚 [Muy largo] {post.Title}",
    };

    /// <summary>
    /// Obtiene color de consola usando pattern matching combinado.
    /// Demuestra: Combinación de patterns.
    /// </summary>
    public ConsoleColor GetStatusColor(TodoResponseDto todo) => todo switch
    {
        { Completed: true } => ConsoleColor.Green,
        { Completed: false, Title: var t } when t.Contains("urgent", StringComparison.OrdinalIgnoreCase) 
            => ConsoleColor.Red,
        { Completed: false } => ConsoleColor.Yellow,
    };

    /// <summary>
    /// Genera resumen usando múltiples patterns.
    /// Demuestra: Deconstrucción y patterns combinados.
    /// </summary>
    public string GenerateSummary(PaginatedResponse<PostResponseDto> response)
    {
        // Deconstrucción implícita no disponible para records con propiedades init-only,
        // pero podemos usar pattern matching de propiedades
        
        return response switch
        {
            { TotalItems: 0 } => "No se encontraron posts.",
            { TotalItems: 1 } => "Se encontró 1 post.",
            { TotalItems: var total, TotalPages: 1 } => $"Se encontraron {total} posts (1 página).",
            { TotalItems: var total, TotalPages: var pages, Page: var page } 
                => $"Se encontraron {total} posts. Página {page} de {pages}.",
        };
    }

    #endregion
}