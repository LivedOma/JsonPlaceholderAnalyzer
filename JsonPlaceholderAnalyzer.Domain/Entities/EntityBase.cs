namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Demuestra: clase abstracta, generics con constraints, init-only properties.
/// </summary>
public abstract class EntityBase<TId> where TId : struct
{
    public required TId Id { get; init; }
    
    public DateTime FetchedAt { get; init; } = DateTime.UtcNow;
    
    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase<TId> other)
            return false;
            
        return Id.Equals(other.Id);
    }
    
    public override int GetHashCode() => Id.GetHashCode();
}