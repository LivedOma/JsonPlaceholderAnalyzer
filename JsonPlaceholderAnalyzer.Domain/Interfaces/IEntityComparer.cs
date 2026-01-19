namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Interfaz para comparar entidades.
/// Demuestra: Contravarianza - solo recibe T como parámetros.
/// Similar a IComparer<T> del framework.
/// </summary>
/// <typeparam name="T">Tipo de entidad a comparar (contravariante)</typeparam>
public interface IEntityComparer<in T>
{
    /// <summary>
    /// Compara dos entidades.
    /// Retorna: negativo si x < y, cero si x == y, positivo si x > y
    /// </summary>
    int Compare(T? x, T? y);
    
    /// <summary>
    /// Verifica si dos entidades son equivalentes según criterio personalizado.
    /// </summary>
    bool AreEquivalent(T? x, T? y);
}