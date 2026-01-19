namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz para procesar datos.
/// Demuestra: CONTRAVARIANZA con 'in' - permite usar IDataProcessor<Base>
/// donde se espera IDataProcessor<Derived>.
/// 
/// La contravarianza (in) significa que T solo aparece en posiciones de ENTRADA (parámetros).
/// Esto permite:
///   IDataProcessor<EntityBase<int>> baseProcessor = ...;
///   IDataProcessor<Post> postProcessor = baseProcessor; // ✓ Válido por contravarianza
/// </summary>
/// <typeparam name="T">Tipo de dato a procesar (contravariante)</typeparam>
public interface IDataProcessor<in T>
{
    /// <summary>
    /// Procesa un elemento.
    /// T está en posición de entrada (parámetro) - compatible con 'in'.
    /// </summary>
    void Process(T item);
    
    /// <summary>
    /// Procesa múltiples elementos.
    /// T está en posición de entrada - compatible con 'in'.
    /// </summary>
    void ProcessMany(IEnumerable<T> items);
    
    /// <summary>
    /// Valida un elemento.
    /// T está en posición de entrada - compatible con 'in'.
    /// </summary>
    bool Validate(T item);
}