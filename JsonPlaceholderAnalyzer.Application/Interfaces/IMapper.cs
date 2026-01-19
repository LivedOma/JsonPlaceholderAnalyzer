namespace JsonPlaceholderAnalyzer.Application.Interfaces;

/// <summary>
/// Interfaz genérica para mapear entre tipos.
/// Demuestra: Generics con múltiples parámetros de tipo, constraints.
/// </summary>
/// <typeparam name="TSource">Tipo de origen (DTO)</typeparam>
/// <typeparam name="TDestination">Tipo de destino (Entidad)</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Mapea un objeto de origen a destino.
    /// </summary>
    TDestination Map(TSource source);
}

/// <summary>
/// Interfaz para mapeo bidireccional.
/// </summary>
/// <typeparam name="TSource">Tipo de origen</typeparam>
/// <typeparam name="TDestination">Tipo de destino</typeparam>
public interface IBidirectionalMapper<TSource, TDestination> 
    : IMapper<TSource, TDestination>
{
    /// <summary>
    /// Mapea de destino a origen (reverso).
    /// </summary>
    TSource MapReverse(TDestination destination);
}