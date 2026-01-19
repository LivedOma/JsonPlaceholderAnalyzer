namespace JsonPlaceholderAnalyzer.Domain.Events;

/// <summary>
/// Argumentos base para eventos de entidades.
/// Demuestra: Herencia de EventArgs, generics, records para inmutabilidad.
/// </summary>
public class EntityEventArgs<T> : EventArgs where T : class
{
    public T Entity { get; }
    public DateTime Timestamp { get; }
    public string? AdditionalInfo { get; init; }

    public EntityEventArgs(T entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Argumentos para eventos de creación de entidades.
/// </summary>
public class EntityCreatedEventArgs<T> : EntityEventArgs<T> where T : class
{
    public int NewId { get; }

    public EntityCreatedEventArgs(T entity, int newId) : base(entity)
    {
        NewId = newId;
    }
}

/// <summary>
/// Argumentos para eventos de actualización de entidades.
/// </summary>
public class EntityUpdatedEventArgs<T> : EntityEventArgs<T> where T : class
{
    public T? OldEntity { get; }

    public EntityUpdatedEventArgs(T entity, T? oldEntity = null) : base(entity)
    {
        OldEntity = oldEntity;
    }
}

/// <summary>
/// Argumentos para eventos de eliminación de entidades.
/// </summary>
public class EntityDeletedEventArgs : EventArgs
{
    public int EntityId { get; }
    public string EntityType { get; }
    public DateTime Timestamp { get; }

    public EntityDeletedEventArgs(int entityId, string entityType)
    {
        EntityId = entityId;
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        Timestamp = DateTime.UtcNow;
    }
}