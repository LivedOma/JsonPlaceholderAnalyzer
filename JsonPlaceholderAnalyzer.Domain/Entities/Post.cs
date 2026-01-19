namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa un post de JSONPlaceholder.
/// Demuestra: herencia de EntityBase, required members.
/// </summary>
public class Post : EntityBase<int>
{
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    
    // Propiedades calculadas
    public int WordCount => Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    public string ShortTitle => Title.Length > 50 ? $"{Title[..47]}..." : Title;
    public string Preview => Body.Length > 100 ? $"{Body[..97]}..." : Body;
}