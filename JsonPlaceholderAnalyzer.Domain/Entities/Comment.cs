namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa un comentario de JSONPlaceholder.
/// Demuestra: herencia, validación básica con propiedades calculadas.
/// </summary>
public class Comment : EntityBase<int>
{
    public required int PostId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Body { get; init; }
    
    // Propiedades calculadas
    public bool HasValidEmail => Email.Contains('@') && Email.Contains('.');
    public int BodyLength => Body.Length;
    public string ShortName => Name.Length > 30 ? $"{Name[..27]}..." : Name;
}