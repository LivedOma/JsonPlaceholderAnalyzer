using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Common;

/// <summary>
/// Ejemplos educativos de cómo usar Result<T> con Pattern Matching.
/// Este archivo es para referencia y aprendizaje.
/// </summary>
public static class ResultUsageExamples
{
    #region 1. Creación de Results

    public static void CreationExamples()
    {
        // Éxito simple
        Result<int> success = Result<int>.Success(42);
        
        // Éxito implícito (usando implicit operator)
        Result<string> implicitSuccess = "Hello";
        
        // Falla general
        Result<int> failure = Result<int>.Failure("Something went wrong");
        
        // Falla con tipo específico
        Result<User> notFound = Result<User>.NotFound("User not found");
        Result<User> validation = Result<User>.ValidationError("Email is invalid");
        
        // Falla desde excepción
        try
        {
            throw new InvalidOperationException("Oops!");
        }
        catch (Exception ex)
        {
            Result<int> fromException = Result<int>.Failure(ex);
        }
        
        // Suprimir warnings de variables no usadas (solo para ejemplo)
        _ = success;
        _ = implicitSuccess;
        _ = failure;
        _ = notFound;
        _ = validation;
    }

    #endregion

    #region 2. Pattern Matching con Result

    public static void PatternMatchingExamples(Result<User> result)
    {
        // Método 1: Deconstrucción básica
        var (isSuccess, value, error) = result;
        if (isSuccess && value is not null)
        {
            Console.WriteLine($"User: {value.Name}");
        }
        else
        {
            Console.WriteLine($"Error: {error}");
        }

        // Método 2: Deconstrucción extendida
        var (success, user, errorMsg, errorType) = result;
        _ = success; _ = user; _ = errorMsg; _ = errorType; // Suprimir warnings
        
        // Método 3: Switch expression con propiedades
        string message = result switch
        {
            { IsSuccess: true, Value.Name: var name } => $"Found user: {name}",
            { IsFailure: true, ErrorType: ErrorType.NotFound } => "User not found",
            { IsFailure: true, ErrorType: ErrorType.Validation } => "Invalid data",
            { IsFailure: true, Error: var err } => $"Error: {err}",
            _ => "Unknown state"
        };
        Console.WriteLine(message);

        // Método 4: Pattern matching con when
        var response = result switch
        {
            { IsSuccess: true, Value: var u } when u is not null && u.Email.Contains("@company.com") 
                => "Internal user",
            { IsSuccess: true } 
                => "External user",
            { ErrorType: ErrorType.NotFound } 
                => "404",
            _ 
                => "Error"
        };
        Console.WriteLine(response);

        // Método 5: Usando Match
        result.Match(
            onSuccess: u => Console.WriteLine($"Found: {u.Name}"),
            onFailure: err => Console.WriteLine($"Failed: {err}")
        );

        // Método 6: Match con valor de retorno
        string displayText = result.Match(
            onSuccess: u => u.DisplayName,
            onFailure: err => $"Error: {err}"
        );
        Console.WriteLine(displayText);
    }

    #endregion

    #region 3. Encadenamiento Funcional

    public static async Task ChainingExamples()
    {
        // Ejemplo de cadena de operaciones
        static Result<int> ParseUserId(string input) =>
            int.TryParse(input, out var id) 
                ? Result<int>.Success(id) 
                : Result<int>.ValidationError("Invalid user ID format");

        static Result<User> GetUser(int id) =>
            id > 0 
                ? Result<User>.Success(CreateSampleUser(id)) 
                : Result<User>.NotFound("User not found");

        static Result<string> ValidateEmail(User user) =>
            user.Email.Contains('@') 
                ? Result<string>.Success(user.Email) 
                : Result<string>.ValidationError("Invalid email");

        // Encadenamiento con Bind
        var result = ParseUserId("123")
            .Bind(GetUser)
            .Bind(ValidateEmail);
        Console.WriteLine($"Bind chain result: {result}");

        // Encadenamiento con Map
        var nameResult = ParseUserId("123")
            .Bind(GetUser)
            .Map(user => user.Name.ToUpper());
        Console.WriteLine($"Map result: {nameResult}");

        // Encadenamiento con Tap (para logging)
        var loggedResult = ParseUserId("123")
            .Tap(id => Console.WriteLine($"Parsed ID: {id}"))
            .Bind(GetUser)
            .Tap(user => Console.WriteLine($"Found user: {user.Name}"))
            .Map(user => user.Email);
        Console.WriteLine($"Logged result: {loggedResult}");

        // Encadenamiento con Ensure
        var validatedResult = ParseUserId("123")
            .Ensure(id => id > 0, "ID must be positive")
            .Ensure(id => id < 1000, "ID too large")
            .Bind(GetUser)
            .Ensure(user => !string.IsNullOrEmpty(user.Email), "User must have email");
        Console.WriteLine($"Validated result: {validatedResult}");

        // Usando GetOrElse
        string email = validatedResult
            .Map(u => u.Email)
            .GetOrElse("no-email@default.com");
        Console.WriteLine($"Email: {email}");

        // Usando Recover - especificando tipo explícitamente
        var recovered = validatedResult
            .Map(u => u.Email)
            .Recover(ErrorType.NotFound, _ => "default@example.com")
            .Recover(_ => "fallback@example.com");
        Console.WriteLine($"Recovered: {recovered}");
        
        await Task.CompletedTask; // Para hacer el método async válido
    }

    #endregion

    #region 4. Combinando Results

    public static void CombiningExamples()
    {
        Result<int> result1 = Result<int>.Success(1);
        Result<string> result2 = Result<string>.Success("hello");
        Result<bool> result3 = Result<bool>.Success(true);

        // Combinar dos results
        Result<(int, string)> combined2 = result1.Combine(result2);
        Console.WriteLine($"Combined 2: {combined2}");
        
        // Combinar tres results
        Result<(int, string, bool)> combined3 = result1.Combine(result2, result3);

        // Pattern matching con tupla combinada
        var message = combined3 switch
        {
            { IsSuccess: true, Value: (var num, var str, var flag) } 
                => $"Got {num}, {str}, {flag}",
            { IsFailure: true, Error: var err } 
                => $"Failed: {err}",
            _ => "Unknown"
        };
        Console.WriteLine($"Combined 3 message: {message}");

        // Combinar colección de Results
        var results = new List<Result<int>>
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        Result<IEnumerable<int>> combinedList = results.Combine();
        Console.WriteLine($"Combined list: {combinedList}");
        
        // Si alguno falla, todo falla
        var resultsWithFailure = new List<Result<int>>
        {
            Result<int>.Success(1),
            Result<int>.Failure("Error!"),
            Result<int>.Success(3)
        };

        Result<IEnumerable<int>> failedList = resultsWithFailure.Combine();
        Console.WriteLine($"Failed list IsFailure: {failedList.IsFailure}");
    }

    #endregion

    #region 5. LINQ-style con Results

    public static void LinqStyleExamples()
    {
        static Result<int> GetNumber() => Result<int>.Success(5);
        static Result<int> Double(int n) => Result<int>.Success(n * 2);
        
        // Usando Select (alias de Map)
        var doubled = GetNumber().Select(n => n * 2);
        Console.WriteLine($"Doubled: {doubled}");
        
        // Usando SelectMany (alias de Bind)
        var result = GetNumber().SelectMany(Double);
        Console.WriteLine($"SelectMany: {result}");
        
        // Usando Where (alias de Ensure)
        var validated = GetNumber()
            .Where(n => n > 0, "Must be positive")
            .Where(n => n < 100, "Must be less than 100");
        Console.WriteLine($"Validated: {validated}");

        // Query syntax (funciona gracias a SelectMany)
        var queryResult = 
            from a in Result<int>.Success(5)
            from b in Result<int>.Success(10)
            select a + b;
        Console.WriteLine($"Query result: {queryResult}");
    }

    #endregion

    #region 6. Async Operations

    public static async Task AsyncExamples()
    {
        static async Task<Result<User>> GetUserAsync(int id)
        {
            await Task.Delay(100); // Simular IO
            return Result<User>.Success(CreateSampleUser(id));
        }

        static async Task<Result<string>> ValidateAsync(User user)
        {
            await Task.Delay(50);
            return Result<string>.Success(user.Email);
        }

        // Async Map
        var result1 = await Result<int>.Success(1)
            .MapAsync(async id =>
            {
                await Task.Delay(100);
                return id * 2;
            });
        Console.WriteLine($"Async Map result: {result1}");

        // Async Bind
        var result2 = await Result<int>.Success(1)
            .BindAsync(GetUserAsync);
        Console.WriteLine($"Async Bind result: {result2}");

        // Cadena async completa
        var userResult = await GetUserAsync(1);
        var result3 = await userResult.BindAsync(ValidateAsync);
        Console.WriteLine($"Async chain result: {result3}");
        
        // TryAsync para operaciones que pueden fallar
        var result4 = await ResultExtensions.TryAsync(async () =>
        {
            await Task.Delay(100);
            return 42;
        });
        Console.WriteLine($"TryAsync result: {result4}");

        // Match async
        var message = await GetUserAsync(1)
            .MatchAsync(
                onSuccess: user => $"Found: {user.Name}",
                onFailure: error => $"Error: {error}"
            );
        Console.WriteLine($"Match async: {message}");
    }

    #endregion

    #region 7. Error Handling Patterns

    public static void ErrorHandlingPatterns(Result<User> result)
    {
        // Pattern 1: Manejo por tipo de error
        result
            .OnErrorType(ErrorType.NotFound, err => 
                Console.WriteLine($"Not found: {err}"))
            .OnErrorType(ErrorType.Validation, err => 
                Console.WriteLine($"Validation error: {err}"))
            .OnErrorType(ErrorType.Network, err => 
                Console.WriteLine($"Network error: {err}"));

        // Pattern 2: Recuperación específica
        var recovered = result
            .Recover(ErrorType.NotFound, _ => CreateSampleUser(0, "Guest"));
        Console.WriteLine($"Recovered: {recovered}");

        // Pattern 3: Switch exhaustivo por tipo de error
        if (result.IsFailure)
        {
            var errorMessage = result.ErrorType switch
            {
                ErrorType.None => "Success!",
                ErrorType.NotFound => "Resource not found - please check the ID",
                ErrorType.Validation => "Invalid data - please check your input",
                ErrorType.Unauthorized => "Access denied - please log in",
                ErrorType.Network => "Network error - please check your connection",
                ErrorType.Timeout => "Request timed out - please try again",
                ErrorType.Exception => $"Unexpected error: {result.Exception?.Message}",
                _ => $"Error: {result.Error}"
            };
            Console.WriteLine(errorMessage);
        }

        // Pattern 4: TapError para logging
        result
            .TapError(error => Console.WriteLine($"[LOG] Error occurred: {error}"));
    }

    #endregion

    #region Helper Methods

    private static User CreateSampleUser(int id, string name = "Test")
    {
        return new User 
        { 
            Id = id, 
            Name = name,
            Username = name.ToLower(),
            Email = $"{name.ToLower()}@test.com",
            Address = new Address("St", "Suite", "City", "12345", new GeoLocation("0", "0")),
            Company = new Company("Co", "Phrase", "bs")
        };
    }

    #endregion
}