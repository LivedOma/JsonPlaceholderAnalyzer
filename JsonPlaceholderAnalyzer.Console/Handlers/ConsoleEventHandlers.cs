using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Domain.Events;

namespace JsonPlaceholderAnalyzer.Console.Handlers;

/// <summary>
/// Manejadores de eventos que escriben en la consola.
/// 
/// Demuestra:
/// - Suscripción a eventos con +=
/// - Desuscripción con -=
/// - Métodos que coinciden con la firma del delegado
/// - Uso de IDisposable para cleanup
/// </summary>
public class ConsoleEventHandlers : IDisposable
{
    private readonly NotificationService _notificationService;
    private bool _disposed = false;
    private bool _verboseMode = false;

    public ConsoleEventHandlers(NotificationService notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        SubscribeToEvents();
    }

    /// <summary>
    /// Activa/desactiva modo verbose para ver todos los logs.
    /// </summary>
    public bool VerboseMode
    {
        get => _verboseMode;
        set => _verboseMode = value;
    }

    /// <summary>
    /// Suscribe todos los manejadores a los eventos.
    /// Demuestra: Suscripción con += usando diferentes sintaxis.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Suscripción usando método nombrado
        _notificationService.EntityCreated += HandleEntityCreated;
        _notificationService.EntityUpdated += HandleEntityUpdated;
        _notificationService.EntityDeleted += HandleEntityDeleted;

        // Suscripción a eventos de API
        _notificationService.ApiRequestStarted += HandleApiRequestStarted;
        _notificationService.ApiRequestCompleted += HandleApiRequestCompleted;
        _notificationService.ApiErrorOccurred += HandleApiError;

        // Suscripción usando delegados personalizados
        _notificationService.NotificationReceived += HandleNotification;
        _notificationService.ProgressUpdated += HandleProgress;
        _notificationService.LogReceived += HandleLog;

        // Suscripción usando lambda (sintaxis alternativa)
        _notificationService.MessageReceived += msg => 
        {
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine($"  💬 {msg}");
            System.Console.ResetColor();
        };

        // Suscripción con Action<T1, T2>
        _notificationService.ColoredMessageReceived += (msg, color) =>
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine($"  {msg}");
            System.Console.ResetColor();
        };
    }

    /// <summary>
    /// Desuscribe todos los manejadores de los eventos.
    /// Demuestra: Desuscripción con -= (importante para evitar memory leaks).
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        _notificationService.EntityCreated -= HandleEntityCreated;
        _notificationService.EntityUpdated -= HandleEntityUpdated;
        _notificationService.EntityDeleted -= HandleEntityDeleted;
        _notificationService.ApiRequestStarted -= HandleApiRequestStarted;
        _notificationService.ApiRequestCompleted -= HandleApiRequestCompleted;
        _notificationService.ApiErrorOccurred -= HandleApiError;
        _notificationService.NotificationReceived -= HandleNotification;
        _notificationService.ProgressUpdated -= HandleProgress;
        _notificationService.LogReceived -= HandleLog;
        
        // Nota: Las lambdas no se pueden desuscribir fácilmente
        // Por eso es mejor usar métodos nombrados cuando necesitas desuscribir
    }

    #region Manejadores de eventos de entidades

    /// <summary>
    /// Manejador para evento EntityCreated.
    /// La firma debe coincidir con EventHandler<EntityCreatedEventArgs<object>>
    /// </summary>
    private void HandleEntityCreated(object? sender, EntityCreatedEventArgs<object> e)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"  ✨ [EVENT] Entity Created: {e.Entity.GetType().Name} (ID: {e.NewId}) at {e.Timestamp:HH:mm:ss}");
        System.Console.ResetColor();
    }

    private void HandleEntityUpdated(object? sender, EntityUpdatedEventArgs<object> e)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"  📝 [EVENT] Entity Updated: {e.Entity.GetType().Name} at {e.Timestamp:HH:mm:ss}");
        System.Console.ResetColor();
    }

    private void HandleEntityDeleted(object? sender, EntityDeletedEventArgs e)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  🗑️ [EVENT] Entity Deleted: {e.EntityType} (ID: {e.EntityId}) at {e.Timestamp:HH:mm:ss}");
        System.Console.ResetColor();
    }

    #endregion

    #region Manejadores de eventos de API

    private void HandleApiRequestStarted(object? sender, ApiRequestEventArgs e)
    {
        if (_verboseMode)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine($"  🌐 [API] {e.HttpMethod} {e.Endpoint}...");
            System.Console.ResetColor();
        }
    }

    private void HandleApiRequestCompleted(object? sender, ApiResponseEventArgs e)
    {
        if (_verboseMode)
        {
            var color = e.IsSuccess ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
            var icon = e.IsSuccess ? "✓" : "✗";
            
            System.Console.ForegroundColor = color;
            System.Console.WriteLine($"  {icon} [API] {e.Endpoint} - {e.Duration.TotalMilliseconds:F0}ms");
            System.Console.ResetColor();
        }
    }

    private void HandleApiError(object? sender, ApiErrorEventArgs e)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  ❌ [API ERROR] {e.Endpoint}: {e.ErrorMessage}");
        if (e.Exception != null && _verboseMode)
        {
            System.Console.WriteLine($"     Exception: {e.Exception.GetType().Name}");
        }
        System.Console.ResetColor();
    }

    #endregion

    #region Manejadores de delegados personalizados

    /// <summary>
    /// Manejador para NotificationHandler.
    /// La firma es: void HandleNotification(string message)
    /// </summary>
    private void HandleNotification(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"  📢 {message}");
        System.Console.ResetColor();
    }

    /// <summary>
    /// Manejador para ProgressHandler.
    /// La firma es: void HandleProgress(int current, int total, string operation)
    /// </summary>
    private void HandleProgress(int current, int total, string operation)
    {
        var percentage = (double)current / total * 100;
        var progressBar = new string('█', (int)(percentage / 5)) + new string('░', 20 - (int)(percentage / 5));
        
        System.Console.ForegroundColor = ConsoleColor.Blue;
        System.Console.Write($"\r  ⏳ [{progressBar}] {percentage:F0}% - {operation}");
        
        if (current == total)
        {
            System.Console.WriteLine(" ✓");
        }
        System.Console.ResetColor();
    }

    /// <summary>
    /// Manejador para LogHandler.
    /// La firma es: void HandleLog(LogLevel level, string message, Exception? exception)
    /// </summary>
    private void HandleLog(LogLevel level, string message, Exception? exception)
    {
        if (!_verboseMode && level == LogLevel.Debug)
            return;

        var (icon, color) = level switch
        {
            LogLevel.Debug => ("🔍", ConsoleColor.DarkGray),
            LogLevel.Info => ("ℹ️", ConsoleColor.Gray),
            LogLevel.Warning => ("⚠️", ConsoleColor.DarkYellow),
            LogLevel.Error => ("❌", ConsoleColor.Red),
            LogLevel.Critical => ("🔥", ConsoleColor.DarkRed),
            _ => ("📝", ConsoleColor.White)
        };

        System.Console.ForegroundColor = color;
        System.Console.WriteLine($"  {icon} [{level}] {message}");
        
        if (exception != null && _verboseMode)
        {
            System.Console.WriteLine($"     {exception.Message}");
        }
        System.Console.ResetColor();
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Importante: desuscribirse de eventos para evitar memory leaks
                UnsubscribeFromEvents();
            }
            _disposed = true;
        }
    }

    #endregion
}