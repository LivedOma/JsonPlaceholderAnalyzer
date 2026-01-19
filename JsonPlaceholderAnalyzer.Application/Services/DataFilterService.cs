using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Events;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio que demuestra el uso de Func<T> y delegados para filtrado.
/// 
/// Demuestra:
/// - Func<T, TResult> como parámetro
/// - Predicate<T> (equivalente a Func<T, bool>)
/// - Action<T> para callbacks
/// - Composición de delegados
/// </summary>
public class DataFilterService
{
    private readonly NotificationService _notificationService;

    public DataFilterService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Filtra una colección usando un predicado (Func<T, bool>).
    /// 
    /// Func<T, bool> es equivalente a Predicate<T>
    /// Recibe un T y retorna bool indicando si el elemento pasa el filtro.
    /// </summary>
    public IEnumerable<T> Filter<T>(
        IEnumerable<T> items, 
        Func<T, bool> predicate,
        Action<T>? onItemFiltered = null)
    {
        var results = new List<T>();
        var total = items.Count();
        var current = 0;

        foreach (var item in items)
        {
            current++;
            
            if (predicate(item))
            {
                results.Add(item);
                onItemFiltered?.Invoke(item);
            }

            // Notificar progreso cada 10 items
            if (current % 10 == 0 || current == total)
            {
                _notificationService.OnProgressUpdate(current, total, "Filtering");
            }
        }

        _notificationService.OnNotification($"Filtered {results.Count} of {total} items");
        return results;
    }

    /// <summary>
    /// Transforma una colección usando una función de mapeo (Func<TSource, TResult>).
    /// </summary>
    public IEnumerable<TResult> Transform<TSource, TResult>(
        IEnumerable<TSource> items,
        Func<TSource, TResult> transformer)
    {
        var results = items.Select(transformer).ToList();
        _notificationService.OnNotification($"Transformed {results.Count} items");
        return results;
    }

    /// <summary>
    /// Agrupa elementos usando un selector de clave (Func<T, TKey>).
    /// </summary>
    public Dictionary<TKey, List<T>> GroupBy<T, TKey>(
        IEnumerable<T> items,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var groups = items
            .GroupBy(keySelector)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        _notificationService.OnNotification($"Grouped into {groups.Count} groups");
        return groups;
    }

    /// <summary>
    /// Ordena elementos usando un selector (Func<T, TKey>).
    /// </summary>
    public IEnumerable<T> OrderBy<T, TKey>(
        IEnumerable<T> items,
        Func<T, TKey> keySelector,
        bool descending = false)
    {
        var ordered = descending
            ? items.OrderByDescending(keySelector)
            : items.OrderBy(keySelector);

        return ordered.ToList();
    }

    /// <summary>
    /// Encuentra el primer elemento que cumple una condición.
    /// </summary>
    public T? FindFirst<T>(
        IEnumerable<T> items,
        Func<T, bool> predicate)
    {
        return items.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Verifica si algún elemento cumple la condición.
    /// </summary>
    public bool Any<T>(
        IEnumerable<T> items,
        Func<T, bool> predicate)
    {
        return items.Any(predicate);
    }

    /// <summary>
    /// Verifica si todos los elementos cumplen la condición.
    /// </summary>
    public bool All<T>(
        IEnumerable<T> items,
        Func<T, bool> predicate)
    {
        return items.All(predicate);
    }

    /// <summary>
    /// Ejecuta una acción para cada elemento.
    /// </summary>
    public void ForEach<T>(
        IEnumerable<T> items,
        Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    /// <summary>
    /// Ejecuta una acción con índice para cada elemento.
    /// Action<T, int> - recibe el elemento y su índice.
    /// </summary>
    public void ForEachWithIndex<T>(
        IEnumerable<T> items,
        Action<T, int> action)
    {
        var index = 0;
        foreach (var item in items)
        {
            action(item, index);
            index++;
        }
    }
}