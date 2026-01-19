using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio completo con operaciones de lectura y escritura.
/// NO puede ser covariante ni contravariante porque T aparece tanto en
/// posiciones de entrada (parámetros) como de salida (retorno).
/// 
/// Hereda de IReadOnlyRepository para las operaciones de lectura.
/// </summary>
/// <typeparam name="T">Tipo de entidad (invariante)</typeparam>
public interface IRepository<T> : IReadOnlyRepository<T> where T : EntityBase<int>
{
    /// <summary>
    /// Crea una nueva entidad.
    /// T aparece como parámetro (entrada) - no compatible con 'out'.
    /// </summary>
    Task<Result<T>> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// T aparece como parámetro (entrada) - no compatible con 'out'.
    /// </summary>
    Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina una entidad por su ID.
    /// </summary>
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}