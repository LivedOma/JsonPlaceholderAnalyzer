namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz para exportar datos.
/// Demuestra contravarianza - solo recibe T como parámetro.
/// </summary>
/// <typeparam name="T">Tipo de dato a exportar (contravariante)</typeparam>
public interface IDataExporter<in T>
{
    /// <summary>
    /// Exporta un elemento a string (para consola, archivo, etc.)
    /// </summary>
    string Export(T item);
    
    /// <summary>
    /// Exporta múltiples elementos.
    /// </summary>
    string ExportMany(IEnumerable<T> items);
    
    /// <summary>
    /// Exporta a un formato específico.
    /// </summary>
    Task ExportToFileAsync(IEnumerable<T> items, string filePath, CancellationToken cancellationToken = default);
}