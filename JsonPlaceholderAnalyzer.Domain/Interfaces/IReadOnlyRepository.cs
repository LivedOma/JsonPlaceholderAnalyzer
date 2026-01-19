using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio de solo lectura.
/// Demuestra: COVARIANZA con 'out' - permite usar IReadOnlyRepository<Derived> 
/// donde se espera IReadOnlyRepository<Base>.
/// 
/// La covarianza (out) significa que T solo aparece en posiciones de SALIDA (retorno).
/// Esto permite:
///   IReadOnlyRepository<Post> postRepo = ...;
///   IReadOnlyRepository<EntityBase<int>> baseRepo = postRepo; // ✓ Válido por covarianza
/// </summary>
/// <typeparam name="T">Tipo de entidad (covariante)</typeparam>
public interface IReadOnlyRepository<out T> where T : EntityBase<int>
{
    /// <summary>
    /// Obtiene todas las entidades.
    /// T está en posición de salida (retorno) - compatible con 'out'.
    /// </summary>
    Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una entidad por su ID.
    /// T está en posición de salida (retorno) - compatible con 'out'.
    /// </summary>
    Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe una entidad con el ID dado.
    /// No usa T en parámetros ni retorno directo.
    /// </summary>
    Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el conteo total de entidades.
    /// </summary>
    Task<Result<int>> CountAsync(CancellationToken cancellationToken = default);
}