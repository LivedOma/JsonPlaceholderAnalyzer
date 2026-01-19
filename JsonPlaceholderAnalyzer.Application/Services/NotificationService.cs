using JsonPlaceholderAnalyzer.Domain.Events;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio centralizado de notificaciones y eventos.
/// 
/// Demuestra:
/// - Eventos con EventHandler<T>
/// - Delegados personalizados
/// - Suscripción/desuscripción (+= / -=)
/// - Invocación segura de eventos (?.Invoke)
/// - Patrón Event Aggregator
/// </summary>
public class NotificationService
{
    #region Eventos de Entidades (usando EventHandler<T> estándar)

    /// <summary>
    /// Se dispara cuando se crea una entidad.
    /// EventHandler<T> es un delegado genérico del framework.
    /// </summary>
    public event EventHandler<EntityCreatedEventArgs<object>>? EntityCreated;

    /// <summary>
    /// Se dispara cuando se actualiza una entidad.
    /// </summary>
    public event EventHandler<EntityUpdatedEventArgs<object>>? EntityUpdated;

    /// <summary>
    /// Se dispara cuando se elimina una entidad.
    /// </summary>
    public event EventHandler<EntityDeletedEventArgs>? EntityDeleted;

    #endregion

    #region Eventos de API (usando EventHandler<T>)

    /// <summary>
    /// Se dispara antes de una llamada a la API.
    /// </summary>
    public event EventHandler<ApiRequestEventArgs>? ApiRequestStarted;

    /// <summary>
    /// Se dispara después de una llamada a la API.
    /// </summary>
    public event EventHandler<ApiResponseEventArgs>? ApiRequestCompleted;

    /// <summary>
    /// Se dispara cuando hay un error en la API.
    /// </summary>
    public event EventHandler<ApiErrorEventArgs>? ApiErrorOccurred;

    #endregion

    #region Eventos con Delegados Personalizados

    /// <summary>
    /// Evento de notificación simple usando delegado personalizado.
    /// </summary>
    public event NotificationHandler? NotificationReceived;

    /// <summary>
    /// Evento de progreso usando delegado personalizado.
    /// </summary>
    public event ProgressHandler? ProgressUpdated;

    /// <summary>
    /// Evento de logging usando delegado personalizado.
    /// </summary>
    public event LogHandler? LogReceived;

    #endregion

    #region Eventos con Action<T> (Delegados genéricos del framework)

    /// <summary>
    /// Evento genérico usando Action<T>.
    /// Action<T> es un delegado que no retorna valor.
    /// </summary>
    public event Action<string>? MessageReceived;

    /// <summary>
    /// Evento con múltiples parámetros usando Action.
    /// </summary>
    public event Action<string, ConsoleColor>? ColoredMessageReceived;

    #endregion

    #region Métodos para disparar eventos de entidades

    /// <summary>
    /// Notifica la creación de una entidad.
    /// Demuestra: Invocación segura con ?.Invoke()
    /// </summary>
    public void OnEntityCreated<T>(T entity, int newId) where T : class
    {
        var args = new EntityCreatedEventArgs<object>(entity, newId);
        
        // Invocación segura: si no hay suscriptores, no hace nada
        EntityCreated?.Invoke(this, args);
        
        // También notificar vía log
        OnLogReceived(LogLevel.Info, $"Entity created: {typeof(T).Name} with ID {newId}");
    }

    /// <summary>
    /// Notifica la actualización de una entidad.
    /// </summary>
    public void OnEntityUpdated<T>(T entity, T? oldEntity = null) where T : class
    {
        var args = new EntityUpdatedEventArgs<object>(entity, oldEntity);
        EntityUpdated?.Invoke(this, args);
        OnLogReceived(LogLevel.Info, $"Entity updated: {typeof(T).Name}");
    }

    /// <summary>
    /// Notifica la eliminación de una entidad.
    /// </summary>
    public void OnEntityDeleted(int entityId, string entityType)
    {
        var args = new EntityDeletedEventArgs(entityId, entityType);
        EntityDeleted?.Invoke(this, args);
        OnLogReceived(LogLevel.Warning, $"Entity deleted: {entityType} with ID {entityId}");
    }

    #endregion

    #region Métodos para disparar eventos de API

    /// <summary>
    /// Notifica el inicio de una llamada a la API.
    /// </summary>
    public void OnApiRequestStarted(string endpoint, string httpMethod)
    {
        var args = new ApiRequestEventArgs(endpoint, httpMethod);
        ApiRequestStarted?.Invoke(this, args);
        OnLogReceived(LogLevel.Debug, $"API Request: {httpMethod} {endpoint}");
    }

    /// <summary>
    /// Notifica la finalización de una llamada a la API.
    /// </summary>
    public void OnApiRequestCompleted(string endpoint, bool isSuccess, TimeSpan duration, int? statusCode = null, string? errorMessage = null)
    {
        var args = new ApiResponseEventArgs(endpoint, isSuccess, duration, statusCode, errorMessage);
        ApiRequestCompleted?.Invoke(this, args);
        
        var level = isSuccess ? LogLevel.Debug : LogLevel.Warning;
        OnLogReceived(level, $"API Response: {endpoint} - {(isSuccess ? "Success" : "Failed")} in {duration.TotalMilliseconds:F0}ms");
    }

    /// <summary>
    /// Notifica un error de la API.
    /// </summary>
    public void OnApiError(string endpoint, string errorMessage, Exception? exception = null)
    {
        var args = new ApiErrorEventArgs(endpoint, errorMessage, exception);
        ApiErrorOccurred?.Invoke(this, args);
        OnLogReceived(LogLevel.Error, $"API Error: {endpoint} - {errorMessage}", exception);
    }

    #endregion

    #region Métodos para disparar eventos con delegados personalizados

    /// <summary>
    /// Envía una notificación simple.
    /// </summary>
    public void OnNotification(string message)
    {
        NotificationReceived?.Invoke(message);
    }

    /// <summary>
    /// Actualiza el progreso de una operación.
    /// </summary>
    public void OnProgressUpdate(int current, int total, string operation)
    {
        ProgressUpdated?.Invoke(current, total, operation);
    }

    /// <summary>
    /// Envía un mensaje de log.
    /// </summary>
    public void OnLogReceived(LogLevel level, string message, Exception? exception = null)
    {
        LogReceived?.Invoke(level, message, exception);
    }

    #endregion

    #region Métodos para disparar eventos con Action<T>

    /// <summary>
    /// Envía un mensaje simple.
    /// </summary>
    public void SendMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Envía un mensaje con color.
    /// </summary>
    public void SendColoredMessage(string message, ConsoleColor color)
    {
        ColoredMessageReceived?.Invoke(message, color);
    }

    #endregion
}