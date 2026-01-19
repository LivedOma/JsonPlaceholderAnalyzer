namespace JsonPlaceholderAnalyzer.Domain.Common;

/// <summary>
/// Extensiones para Result<T> que proporcionan operaciones adicionales.
/// 
/// Demuestra:
/// - Extension Methods
/// - Métodos genéricos
/// - Pattern Matching en extensiones
/// - Async extensions
/// </summary>
public static class ResultExtensions
{
    #region Combining Results

    /// <summary>
    /// Combina dos Results en un tuple si ambos son exitosos.
    /// </summary>
    public static Result<(T1, T2)> Combine<T1, T2>(
        this Result<T1> first, 
        Result<T2> second)
    {
        return (first.IsSuccess, second.IsSuccess) switch
        {
            (true, true) => Result<(T1, T2)>.Success((first.Value!, second.Value!)),
            (false, _) => Result<(T1, T2)>.Failure(first.Error!, first.ErrorType),
            (_, false) => Result<(T1, T2)>.Failure(second.Error!, second.ErrorType)
        };
    }

    /// <summary>
    /// Combina tres Results.
    /// </summary>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(
        this Result<T1> first,
        Result<T2> second,
        Result<T3> third)
    {
        return (first.IsSuccess, second.IsSuccess, third.IsSuccess) switch
        {
            (true, true, true) => Result<(T1, T2, T3)>.Success((first.Value!, second.Value!, third.Value!)),
            (false, _, _) => Result<(T1, T2, T3)>.Failure(first.Error!, first.ErrorType),
            (_, false, _) => Result<(T1, T2, T3)>.Failure(second.Error!, second.ErrorType),
            (_, _, false) => Result<(T1, T2, T3)>.Failure(third.Error!, third.ErrorType)
        };
    }

    /// <summary>
    /// Combina una colección de Results en un Result de colección.
    /// </summary>
    public static Result<IEnumerable<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        var resultList = results.ToList();
        var failures = resultList.Where(r => r.IsFailure).ToList();
        
        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(f => f.Error));
            return Result<IEnumerable<T>>.Failure(errors);
        }
        
        return Result<IEnumerable<T>>.Success(resultList.Select(r => r.Value!));
    }

    #endregion

    #region Async Extensions

    /// <summary>
    /// Map asíncrono.
    /// </summary>
    public static async Task<Result<TNew>> MapAsync<T, TNew>(
        this Result<T> result,
        Func<T, Task<TNew>> mapper)
    {
        if (result.IsFailure)
            return Result<TNew>.Failure(result.Error!, result.ErrorType);
        
        try
        {
            var newValue = await mapper(result.Value!);
            return Result<TNew>.Success(newValue);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// Map asíncrono para Task<Result<T>>.
    /// </summary>
    public static async Task<Result<TNew>> MapAsync<T, TNew>(
        this Task<Result<T>> resultTask,
        Func<T, TNew> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Bind asíncrono.
    /// </summary>
    public static async Task<Result<TNew>> BindAsync<T, TNew>(
        this Result<T> result,
        Func<T, Task<Result<TNew>>> binder)
    {
        if (result.IsFailure)
            return Result<TNew>.Failure(result.Error!, result.ErrorType);
        
        try
        {
            return await binder(result.Value!);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// Bind asíncrono para Task<Result<T>>.
    /// </summary>
    public static async Task<Result<TNew>> BindAsync<T, TNew>(
        this Task<Result<T>> resultTask,
        Func<T, Result<TNew>> binder)
    {
        var result = await resultTask;
        return result.Bind(binder);
    }

    /// <summary>
    /// Tap asíncrono.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action)
    {
        var result = await resultTask;
        return result.Tap(action);
    }

    /// <summary>
    /// Match asíncrono.
    /// </summary>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> resultTask,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    #endregion

    #region Conversion Extensions

    /// <summary>
    /// Convierte un valor nullable a Result.
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, string errorIfNull = "Value is null") 
        where T : class
    {
        return value is not null 
            ? Result<T>.Success(value) 
            : Result<T>.Failure(errorIfNull, ErrorType.NotFound);
    }

    /// <summary>
    /// Convierte un valor nullable (struct) a Result.
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, string errorIfNull = "Value is null") 
        where T : struct
    {
        return value.HasValue 
            ? Result<T>.Success(value.Value) 
            : Result<T>.Failure(errorIfNull, ErrorType.NotFound);
    }

    /// <summary>
    /// Intenta ejecutar una acción y retorna Result.
    /// </summary>
    public static Result<T> Try<T>(Func<T> action)
    {
        try
        {
            return Result<T>.Success(action());
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    /// Intenta ejecutar una acción asíncrona y retorna Result.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Result<T>.Success(await action());
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    #endregion

    #region LINQ-style Extensions

    /// <summary>
    /// Where para Result (Ensure con sintaxis de LINQ).
    /// </summary>
    public static Result<T> Where<T>(
        this Result<T> result, 
        Func<T, bool> predicate, 
        string errorMessage = "Predicate not satisfied")
    {
        return result.Ensure(predicate, errorMessage);
    }

    /// <summary>
    /// Select para Result (alias de Map).
    /// </summary>
    public static Result<TNew> Select<T, TNew>(
        this Result<T> result, 
        Func<T, TNew> selector)
    {
        return result.Map(selector);
    }

    /// <summary>
    /// SelectMany para Result (alias de Bind).
    /// </summary>
    public static Result<TNew> SelectMany<T, TNew>(
        this Result<T> result, 
        Func<T, Result<TNew>> selector)
    {
        return result.Bind(selector);
    }

    /// <summary>
    /// SelectMany con proyección final.
    /// </summary>
    public static Result<TResult> SelectMany<T, TIntermediate, TResult>(
        this Result<T> result,
        Func<T, Result<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
    {
        return result.Bind(x => 
            intermediateSelector(x).Map(y => 
                resultSelector(x, y)));
    }

    #endregion

    #region Pattern Matching Helpers

    /// <summary>
    /// Verifica si el error es de un tipo específico.
    /// </summary>
    public static bool HasErrorType<T>(this Result<T> result, ErrorType errorType)
    {
        return result.IsFailure && result.ErrorType == errorType;
    }

    /// <summary>
    /// Ejecuta acción específica según el tipo de error.
    /// </summary>
    public static Result<T> OnErrorType<T>(
        this Result<T> result, 
        ErrorType errorType, 
        Action<string> action)
    {
        if (result.HasErrorType(errorType))
        {
            action(result.Error ?? "Unknown error");
        }
        return result;
    }

    /// <summary>
    /// Recupera de un error específico.
    /// </summary>
    public static Result<T> Recover<T>(
        this Result<T> result,
        ErrorType errorType,
        Func<string, T> recovery)
    {
        if (result.HasErrorType(errorType))
        {
            return Result<T>.Success(recovery(result.Error ?? "Unknown error"));
        }
        return result;
    }

    /// <summary>
    /// Recupera de cualquier error.
    /// </summary>
    public static Result<T> Recover<T>(
        this Result<T> result,
        Func<string, T> recovery)
    {
        if (result.IsFailure)
        {
            return Result<T>.Success(recovery(result.Error ?? "Unknown error"));
        }
        return result;
    }

    #endregion
}