namespace JsonPlaceholderAnalyzer.Application.DTOs;

/// <summary>
/// DTOs de respuesta para presentar datos al usuario.
/// 
/// Demuestra:
/// - Records para inmutabilidad
/// - Init-only properties
/// - Propiedades calculadas en records
/// - With-expressions capability
/// </summary>

/// <summary>
/// DTO de respuesta para usuario.
/// </summary>
public record UserResponseDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string City { get; init; }
    public required string CompanyName { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    
    // Propiedad calculada
    public string DisplayName => $"{Name} (@{Username})";
    public bool HasContactInfo => !string.IsNullOrEmpty(Phone) || !string.IsNullOrEmpty(Website);
}

/// <summary>
/// DTO de respuesta para post.
/// </summary>
public record PostResponseDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public string? AuthorName { get; init; }
    
    // Propiedades calculadas
    public int WordCount => Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    public string Preview => Body.Length > 100 ? $"{Body[..97]}..." : Body;
    public PostLength Length => WordCount switch
    {
        < 20 => PostLength.Short,
        < 50 => PostLength.Medium,
        < 100 => PostLength.Long,
        _ => PostLength.VeryLong
    };
}

/// <summary>
/// Enum para clasificar longitud de posts.
/// </summary>
public enum PostLength
{
    Short,
    Medium,
    Long,
    VeryLong
}

/// <summary>
/// DTO de respuesta para comentario.
/// </summary>
public record CommentResponseDto
{
    public required int Id { get; init; }
    public required int PostId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Body { get; init; }
    
    public string ShortName => Name.Length > 30 ? $"{Name[..27]}..." : Name;
    public bool IsValidEmail => Email.Contains('@') && Email.Contains('.');
}

/// <summary>
/// DTO de respuesta para todo.
/// </summary>
public record TodoResponseDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public required bool Completed { get; init; }
    public string? UserName { get; init; }
    
    public string Status => Completed ? "✓ Completado" : "○ Pendiente";
    public string StatusEmoji => Completed ? "✅" : "⏳";
}

/// <summary>
/// DTO de respuesta para álbum.
/// </summary>
public record AlbumResponseDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public int PhotoCount { get; init; }
    public string? UserName { get; init; }
}

/// <summary>
/// DTO de respuesta para foto.
/// </summary>
public record PhotoResponseDto
{
    public required int Id { get; init; }
    public required int AlbumId { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string ThumbnailUrl { get; init; }
    
    public string ShortTitle => Title.Length > 40 ? $"{Title[..37]}..." : Title;
}