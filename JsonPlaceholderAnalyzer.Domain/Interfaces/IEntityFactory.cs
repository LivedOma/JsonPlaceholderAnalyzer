namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz de fábrica para crear entidades.
/// Demuestra: Covarianza - solo retorna T, no lo recibe como parámetro.
/// Similar a IEnumerable<T> del framework.
/// </summary>
/// <typeparam name="T">Tipo de entidad a crear (covariante)</typeparam>
public interface IEntityFactory<out T> where T : EntityBase<int>
{
    /// <summary>
    /// Crea una entidad con valores por defecto.
    /// T está en posición de salida - compatible con 'out'.
    /// </summary>
    T CreateDefault();
    
    /// <summary>
    /// Crea una entidad desde un diccionario de valores.
    /// T está en posición de salida - compatible con 'out'.
    /// </summary>
    T CreateFromDictionary(IDictionary<string, object> values);
}