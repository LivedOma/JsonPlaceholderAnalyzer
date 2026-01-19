namespace JsonPlaceholderAnalyzer.Domain.Events;

/// <summary>
/// Delegados personalizados para el sistema de eventos.
/// 
/// Demuestra: Definición de delegados custom.
/// 
/// Un DELEGADO es un tipo que representa referencias a métodos.
/// Es como un "puntero a función" type-safe en C#.
/// 
/// Sintaxis: delegate TReturn NombreDelegate(TParam1 param1, TParam2 param2, ...);
/// </summary>

// Delegado para notificaciones simples (sin parámetros de evento)
public delegate void NotificationHandler(string message);

// Delegado para progreso de operaciones
public delegate void ProgressHandler(int current, int total, string operation);

// Delegado para validación (retorna bool)
public delegate bool ValidationHandler<T>(T item, out string? errorMessage);

// Delegado para transformación de datos
public delegate TResult TransformHandler<TSource, TResult>(TSource source);

// Delegado para filtrado
public delegate bool FilterHandler<T>(T item);

// Delegado para logging con nivel
public delegate void LogHandler(LogLevel level, string message, Exception? exception = null);

/// <summary>
/// Niveles de log para el sistema de notificaciones.
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}