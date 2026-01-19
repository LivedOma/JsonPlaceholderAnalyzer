using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity, TDto>(
    IApiClient apiClient,
    IMapper<TDto, TEntity> mapper,
    string resourceEndpoint
) : IRepository<TEntity>
    where TEntity : EntityBase<int>
    where TDto : class
{
    protected readonly IApiClient ApiClient = apiClient;
    protected readonly IMapper<TDto, TEntity> Mapper = mapper;
    protected readonly string ResourceEndpoint = resourceEndpoint;
    
    private readonly Dictionary<int, TEntity> _localCache = new();
    private bool _cacheInitialized = false;

    #region IReadOnlyRepository Implementation

    public virtual async Task<Result<IEnumerable<TEntity>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cacheInitialized && _localCache.Count > 0)
        {
            return Result<IEnumerable<TEntity>>.Success(_localCache.Values);
        }
        
        var result = await ApiClient.GetListAsync<TDto>(ResourceEndpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => MapAndCacheResults(result.Value),
            { IsFailure: true } => Result<IEnumerable<TEntity>>.Failure(result.Error ?? "Unknown error"),
            _ => Result<IEnumerable<TEntity>>.Failure("Unexpected result state")
        };
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        if (_localCache.TryGetValue(id, out var cachedEntity))
        {
            return Result<TEntity>.Success(cachedEntity);
        }
        
        var endpoint = $"{ResourceEndpoint}/{id}";
        var result = await ApiClient.GetAsync<TDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => MapAndCacheSingle(result.Value),
            { IsFailure: true } => Result<TEntity>.Failure(result.Error ?? "Not found"),
            _ => Result<TEntity>.Failure($"Entity with ID {id} not found")
        };
    }

    public virtual async Task<Result<bool>> ExistsAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        if (_localCache.ContainsKey(id))
        {
            return Result<bool>.Success(true);
        }
        
        var result = await GetByIdAsync(id, cancellationToken);
        return Result<bool>.Success(result.IsSuccess);
    }

    public virtual async Task<Result<int>> CountAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheInitialized)
        {
            return Result<int>.Success(_localCache.Count);
        }
        
        var result = await GetAllAsync(cancellationToken);
        return result.IsSuccess
            ? Result<int>.Success(result.Value?.Count() ?? 0)
            : Result<int>.Failure(result.Error ?? "Failed to count");
    }

    #endregion

    #region IRepository Implementation

    public virtual async Task<Result<TEntity>> CreateAsync(
        TEntity entity, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var maxId = _localCache.Keys.DefaultIfEmpty(0).Max();
        var newId = maxId + 1;
        
        var newEntity = CloneEntityWithNewId(entity, newId);
        
        _localCache[newId] = newEntity;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  📝 Created {typeof(TEntity).Name} with ID: {newId} (simulated)");
        Console.ResetColor();
        
        return await Task.FromResult(Result<TEntity>.Success(newEntity));
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(
        TEntity entity, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        if (!_localCache.ContainsKey(entity.Id) && !await ExistsInApiAsync(entity.Id, cancellationToken))
        {
            return Result<TEntity>.Failure($"Entity with ID {entity.Id} not found");
        }
        
        _localCache[entity.Id] = entity;
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  📝 Updated {typeof(TEntity).Name} with ID: {entity.Id} (simulated)");
        Console.ResetColor();
        
        return await Task.FromResult(Result<TEntity>.Success(entity));
    }

    public virtual async Task<Result> DeleteAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        if (!_localCache.ContainsKey(id) && !await ExistsInApiAsync(id, cancellationToken))
        {
            return Result.Failure($"Entity with ID {id} not found");
        }
        
        _localCache.Remove(id);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  🗑️ Deleted {typeof(TEntity).Name} with ID: {id} (simulated)");
        Console.ResetColor();
        
        return await Task.FromResult(Result.Success());
    }

    #endregion

    #region Protected Helper Methods

    protected Result<IEnumerable<TEntity>> MapAndCacheResults(IEnumerable<TDto> dtos)
    {
        var entities = dtos.Select(dto => Mapper.Map(dto)).ToList();
        
        foreach (var entity in entities)
        {
            _localCache[entity.Id] = entity;
        }
        _cacheInitialized = true;
        
        return Result<IEnumerable<TEntity>>.Success(entities);
    }

    protected Result<TEntity> MapAndCacheSingle(TDto dto)
    {
        var entity = Mapper.Map(dto);
        _localCache[entity.Id] = entity;
        return Result<TEntity>.Success(entity);
    }

    protected async Task<bool> ExistsInApiAsync(int id, CancellationToken cancellationToken)
    {
        var endpoint = $"{ResourceEndpoint}/{id}";
        var result = await ApiClient.GetAsync<TDto>(endpoint, cancellationToken);
        return result.IsSuccess;
    }

    private static TEntity CloneEntityWithNewId(TEntity source, int newId)
    {
        var type = typeof(TEntity);
        var clone = (TEntity)Activator.CreateInstance(type)!;
        
        foreach (var prop in type.GetProperties())
        {
            if (prop.CanWrite)
            {
                if (prop.Name == "Id")
                {
                    prop.SetValue(clone, newId);
                }
                else
                {
                    prop.SetValue(clone, prop.GetValue(source));
                }
            }
        }
        
        return clone;
    }

    public void ClearCache()
    {
        _localCache.Clear();
        _cacheInitialized = false;
    }

    #endregion
}