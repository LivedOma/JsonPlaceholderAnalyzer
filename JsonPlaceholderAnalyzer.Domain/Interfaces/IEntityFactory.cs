namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz de fábrica para crear entidades.
/// 
/// NOTA EDUCATIVA: Esta interfaz SÍ podría ser covariante (out T)
/// porque solo retorna T directamente, no envuelto en Result<T>.
/// Sin embargo, para mantener consistencia con las demás interfaces,
/// la dejamos invariante.
/// </summary>
/// <typeparam name="T">Tipo de entidad a crear</typeparam>
public interface IEntityFactory<T> where T : EntityBase<int>
{
    /// <summary>
    /// Crea una entidad con valores por defecto.
    /// </summary>
    T CreateDefault();
    
    /// <summary>
    /// Crea una entidad desde un diccionario de valores.
    /// </summary>
    T CreateFromDictionary(IDictionary<string, object> values);
}