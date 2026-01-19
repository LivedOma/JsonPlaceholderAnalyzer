using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz de repositorio de solo lectura.
/// 
/// NOTA EDUCATIVA sobre COVARIANZA:
/// Originalmente queríamos usar 'out T' para hacer esta interfaz covariante,
/// pero no es posible porque Result<T> no es covariante.
/// 
/// Para que una interfaz sea covariante (out T), TODOS los tipos que usen T
/// en posición de retorno también deben ser covariantes.
/// 
/// Result<T> es invariante porque es una clase, no una interfaz con 'out'.
/// Por eso removemos 'out' de esta interfaz.
/// 
/// Ejemplo de interfaz covariante válida: IEnumerable<out T>
/// (porque solo retorna T, nunca lo recibe como parámetro)
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IReadOnlyRepository<T> where T : EntityBase<int>
{
    /// <summary>
    /// Obtiene todas las entidades.
    /// </summary>
    Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene una entidad por su ID.
    /// </summary>
    Task<Result<T>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si existe una entidad con el ID dado.
    /// </summary>
    Task<Result<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el conteo total de entidades.
    /// </summary>
    Task<Result<int>> CountAsync(CancellationToken cancellationToken = default);
}