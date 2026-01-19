using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Clase base abstracta para servicios de entidades.
/// 
/// Demuestra:
/// - Primary Constructor (C# 12)
/// - Clase abstracta con implementación parcial
/// - Métodos virtuales para override
/// - Inyección de dependencias vía constructor
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
/// <typeparam name="TRepository">Tipo de repositorio</typeparam>
public abstract class EntityServiceBase<T, TRepository>(
    TRepository repository,
    NotificationService notificationService
) : IEntityService<T>
    where T : EntityBase<int>
    where TRepository : IRepository<T>
{
    // Campos protegidos accesibles por clases derivadas
    protected readonly TRepository Repository = repository;
    protected readonly NotificationService NotificationService = notificationService;

    /// <summary>
    /// Obtiene todas las entidades.
    /// Virtual para permitir override en clases derivadas.
    /// </summary>
    public virtual async Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        NotificationService.OnLogReceived(Domain.Events.LogLevel.Debug, $"Getting all {typeof(T).Name}s");
        
        var result = await Repository.GetAllAsync(cancellationToken);
        
        if (result.IsSuccess)
        {
            NotificationService.OnLogReceived(
                Domain.Events.LogLevel.Info, 
                $"Retrieved {result.Value?.Count() ?? 0} {typeof(T).Name}s"
            );
        }
        
        return result;
    }

    /// <summary>
    /// Obtiene una entidad por ID.
    /// </summary>
    public virtual async Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        NotificationService.OnLogReceived(Domain.Events.LogLevel.Debug, $"Getting {typeof(T).Name} with ID {id}");
        
        var result = await Repository.GetByIdAsync(id, cancellationToken);
        
        if (result.IsFailure)
        {
            NotificationService.OnLogReceived(
                Domain.Events.LogLevel.Warning, 
                $"{typeof(T).Name} with ID {id} not found"
            );
        }
        
        return result;
    }

    /// <summary>
    /// Obtiene el conteo total de entidades.
    /// </summary>
    public virtual async Task<Result<int>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Repository.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Crea una nueva entidad.
    /// </summary>
    public virtual async Task<Result<T>> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Validación antes de crear
        var validationResult = ValidateEntity(entity);
        if (validationResult.IsFailure)
        {
            return Result<T>.Failure(validationResult.Error ?? "Validation failed");
        }
        
        var result = await Repository.CreateAsync(entity, cancellationToken);
        
        if (result.IsSuccess)
        {
            NotificationService.OnEntityCreated(result.Value!, result.Value!.Id);
        }
        
        return result;
    }

    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    public virtual async Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // Validación antes de actualizar
        var validationResult = ValidateEntity(entity);
        if (validationResult.IsFailure)
        {
            return Result<T>.Failure(validationResult.Error ?? "Validation failed");
        }
        
        // Obtener entidad original para el evento
        var originalResult = await Repository.GetByIdAsync(entity.Id, cancellationToken);
        
        var result = await Repository.UpdateAsync(entity, cancellationToken);
        
        if (result.IsSuccess)
        {
            NotificationService.OnEntityUpdated(result.Value!, originalResult.Value);
        }
        
        return result;
    }

    /// <summary>
    /// Elimina una entidad.
    /// </summary>
    public virtual async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await Repository.DeleteAsync(id, cancellationToken);
        
        if (result.IsSuccess)
        {
            NotificationService.OnEntityDeleted(id, typeof(T).Name);
        }
        
        return result;
    }

    /// <summary>
    /// Método abstracto para validación específica de cada entidad.
    /// Las clases derivadas DEBEN implementar este método.
    /// </summary>
    protected abstract Result ValidateEntity(T entity);

    /// <summary>
    /// Método virtual para lógica adicional antes de guardar.
    /// Las clases derivadas PUEDEN override este método.
    /// </summary>
    protected virtual Task<Result> BeforeSaveAsync(T entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Método virtual para lógica adicional después de guardar.
    /// </summary>
    protected virtual Task AfterSaveAsync(T entity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}