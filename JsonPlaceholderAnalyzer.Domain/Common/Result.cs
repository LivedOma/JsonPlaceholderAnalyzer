namespace JsonPlaceholderAnalyzer.Domain.Common;

/// <summary>
/// Representa el resultado de una operación que puede fallar.
/// Versión mejorada con métodos funcionales y pattern matching.
/// 
/// Demuestra:
/// - Pattern Matching friendly con Deconstruct
/// - Métodos funcionales (Map, Bind, Match)
/// - Implicit operators
/// - Expression-bodied members
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public Exception? Exception { get; }
    public ErrorType ErrorType { get; }

    #region Constructors

    private Result(bool isSuccess, T? value, string? error, Exception? exception, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Exception = exception;
        ErrorType = errorType;
    }

    #endregion

    #region Factory Methods

    public static Result<T> Success(T value) => 
        new(true, value, null, null, ErrorType.None);
    
    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.General) => 
        new(false, default, error, null, errorType);
    
    public static Result<T> Failure(Exception exception, ErrorType errorType = ErrorType.Exception) => 
        new(false, default, exception.Message, exception, errorType);
    
    public static Result<T> NotFound(string message = "Resource not found") => 
        new(false, default, message, null, ErrorType.NotFound);
    
    public static Result<T> ValidationError(string message) => 
        new(false, default, message, null, ErrorType.Validation);
    
    public static Result<T> Unauthorized(string message = "Unauthorized access") => 
        new(false, default, message, null, ErrorType.Unauthorized);

    #endregion

    #region Deconstruction for Pattern Matching

    /// <summary>
    /// Permite deconstruir para pattern matching básico.
    /// Uso: var (success, value, error) = result;
    /// </summary>
    public void Deconstruct(out bool isSuccess, out T? value, out string? error)
    {
        isSuccess = IsSuccess;
        value = Value;
        error = Error;
    }

    /// <summary>
    /// Deconstrucción extendida con tipo de error.
    /// Uso: var (success, value, error, errorType) = result;
    /// </summary>
    public void Deconstruct(out bool isSuccess, out T? value, out string? error, out ErrorType errorType)
    {
        isSuccess = IsSuccess;
        value = Value;
        error = Error;
        errorType = ErrorType;
    }

    #endregion

    #region Functional Methods

    /// <summary>
    /// MAP: Transforma el valor si es exitoso, preserva el error si falla.
    /// Equivalente a Select en LINQ.
    /// 
    /// Uso: result.Map(x => x.ToString())
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!, ErrorType);
        
        try
        {
            var newValue = mapper(Value!);
            return Result<TNew>.Success(newValue);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// BIND (FlatMap): Transforma el valor con una función que retorna Result.
    /// Equivalente a SelectMany en LINQ.
    /// 
    /// Uso: result.Bind(x => GetOtherResult(x))
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Error!, ErrorType);
        
        try
        {
            return binder(Value!);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// MATCH: Ejecuta una acción dependiendo del estado.
    /// Pattern matching funcional.
    /// 
    /// Uso: result.Match(
    ///     onSuccess: value => Console.WriteLine(value),
    ///     onFailure: error => Console.WriteLine(error)
    /// )
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess 
            ? onSuccess(Value!) 
            : onFailure(Error ?? "Unknown error");
    }

    /// <summary>
    /// MATCH con acciones (sin valor de retorno).
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(Error ?? "Unknown error");
    }

    /// <summary>
    /// TAP: Ejecuta una acción si es exitoso, retorna el mismo Result.
    /// Útil para logging o side effects.
    /// 
    /// Uso: result.Tap(x => Console.WriteLine(x))
    /// </summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        
        return this;
    }

    /// <summary>
    /// TAPERROR: Ejecuta una acción si falla.
    /// </summary>
    public Result<T> TapError(Action<string> action)
    {
        if (IsFailure)
            action(Error ?? "Unknown error");
        
        return this;
    }

    /// <summary>
    /// ENSURE: Agrega una validación adicional.
    /// </summary>
    public Result<T> Ensure(Func<T, bool> predicate, string errorMessage)
    {
        if (IsFailure)
            return this;
        
        return predicate(Value!) 
            ? this 
            : Failure(errorMessage, ErrorType.Validation);
    }

    /// <summary>
    /// GETORELSE: Obtiene el valor o un valor por defecto.
    /// </summary>
    public T GetOrElse(T defaultValue) => 
        IsSuccess ? Value! : defaultValue;

    /// <summary>
    /// GETORELSE con función.
    /// </summary>
    public T GetOrElse(Func<T> defaultValueFactory) => 
        IsSuccess ? Value! : defaultValueFactory();

    /// <summary>
    /// GETORTHROW: Obtiene el valor o lanza excepción.
    /// </summary>
    public T GetOrThrow()
    {
        if (IsFailure)
            throw Exception ?? new InvalidOperationException(Error ?? "Operation failed");
        
        return Value!;
    }

    #endregion

    #region Implicit Operators

    /// <summary>
    /// Permite crear Result<T>.Success(value) implícitamente desde T.
    /// Uso: Result<int> result = 42;
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    #endregion

    #region Object Overrides

    public override string ToString() => IsSuccess 
        ? $"Success({Value})" 
        : $"Failure({Error})";

    #endregion
}

/// <summary>
/// Resultado sin valor de retorno (para operaciones void).
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public Exception? Exception { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, string? error, Exception? exception, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        Exception = exception;
        ErrorType = errorType;
    }

    public static Result Success() => 
        new(true, null, null, ErrorType.None);
    
    public static Result Failure(string error, ErrorType errorType = ErrorType.General) => 
        new(false, error, null, errorType);
    
    public static Result Failure(Exception exception, ErrorType errorType = ErrorType.Exception) => 
        new(false, exception.Message, exception, errorType);

    public void Deconstruct(out bool isSuccess, out string? error)
    {
        isSuccess = IsSuccess;
        error = Error;
    }

    /// <summary>
    /// Convierte a Result<T>.
    /// </summary>
    public Result<T> ToResult<T>(T value) => 
        IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(Error!, ErrorType);

    public override string ToString() => IsSuccess 
        ? "Success" 
        : $"Failure({Error})";
}

/// <summary>
/// Tipos de error para mejor categorización.
/// </summary>
public enum ErrorType
{
    None = 0,
    General = 1,
    Validation = 2,
    NotFound = 3,
    Unauthorized = 4,
    Conflict = 5,
    Exception = 6,
    Network = 7,
    Timeout = 8
}