namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa un álbum de JSONPlaceholder.
/// </summary>
public class Album : EntityBase<int>
{
    public required int UserId { get; init; }
    public required string Title { get; init; }
    
    // Navegación (se llenará opcionalmente)
    public IReadOnlyList<Photo> Photos { get; init; } = [];
    
    public int PhotoCount => Photos.Count;
}