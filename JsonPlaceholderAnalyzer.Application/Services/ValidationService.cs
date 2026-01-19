using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio de validación que retorna Result para cada validación.
/// Demuestra uso práctico de Result<T> para validaciones.
/// </summary>
public class ValidationService
{
    #region User Validation

    public Result<User> ValidateUser(User? user)
    {
        if (user is null)
            return Result<User>.ValidationError("User cannot be null");

        return Result<User>.Success(user)
            .Ensure(u => !string.IsNullOrWhiteSpace(u.Name), "Name is required")
            .Ensure(u => !string.IsNullOrWhiteSpace(u.Username), "Username is required")
            .Ensure(u => u.Username.Length >= 3, "Username must be at least 3 characters")
            .Ensure(u => !string.IsNullOrWhiteSpace(u.Email), "Email is required")
            .Ensure(u => IsValidEmail(u.Email), "Email format is invalid")
            .Ensure(u => u.Address is not null, "Address is required")
            .Ensure(u => u.Company is not null, "Company is required");
    }

    public Result<string> ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<string>.ValidationError("Username cannot be empty");

        return Result<string>.Success(username)
            .Ensure(u => u.Length >= 3, "Username must be at least 3 characters")
            .Ensure(u => u.Length <= 20, "Username cannot exceed 20 characters")
            .Ensure(u => u.All(c => char.IsLetterOrDigit(c) || c == '_'), 
                "Username can only contain letters, numbers, and underscores");
    }

    public Result<string> ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<string>.ValidationError("Email cannot be empty");

        return IsValidEmail(email)
            ? Result<string>.Success(email)
            : Result<string>.ValidationError("Invalid email format");
    }

    #endregion

    #region Post Validation

    public Result<Post> ValidatePost(Post? post)
    {
        if (post is null)
            return Result<Post>.ValidationError("Post cannot be null");

        return Result<Post>.Success(post)
            .Ensure(p => p.UserId > 0, "UserId must be a positive number")
            .Ensure(p => !string.IsNullOrWhiteSpace(p.Title), "Title is required")
            .Ensure(p => p.Title.Length <= 200, "Title cannot exceed 200 characters")
            .Ensure(p => !string.IsNullOrWhiteSpace(p.Body), "Body is required")
            .Ensure(p => p.Body.Length >= 10, "Body must be at least 10 characters");
    }

    public Result<string> ValidatePostTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<string>.ValidationError("Title cannot be empty");

        return Result<string>.Success(title)
            .Ensure(t => t.Length >= 5, "Title must be at least 5 characters")
            .Ensure(t => t.Length <= 200, "Title cannot exceed 200 characters")
            .Ensure(t => !t.All(char.IsDigit), "Title cannot be all numbers");
    }

    #endregion

    #region Todo Validation

    public Result<Todo> ValidateTodo(Todo? todo)
    {
        if (todo is null)
            return Result<Todo>.ValidationError("Todo cannot be null");

        return Result<Todo>.Success(todo)
            .Ensure(t => t.UserId > 0, "UserId must be a positive number")
            .Ensure(t => !string.IsNullOrWhiteSpace(t.Title), "Title is required")
            .Ensure(t => t.Title.Length <= 200, "Title cannot exceed 200 characters");
    }

    #endregion

    #region Generic Validations

    public Result<int> ValidateId(int id, string entityName = "Entity")
    {
        return id switch
        {
            <= 0 => Result<int>.ValidationError($"{entityName} ID must be a positive number"),
            > 10000 => Result<int>.ValidationError($"{entityName} ID is out of range"),
            _ => Result<int>.Success(id)
        };
    }

    public Result<int> ValidatePage(int page)
    {
        return page switch
        {
            < 1 => Result<int>.ValidationError("Page must be at least 1"),
            > 1000 => Result<int>.ValidationError("Page cannot exceed 1000"),
            _ => Result<int>.Success(page)
        };
    }

    public Result<int> ValidatePageSize(int pageSize)
    {
        return pageSize switch
        {
            < 1 => Result<int>.ValidationError("Page size must be at least 1"),
            > 100 => Result<int>.ValidationError("Page size cannot exceed 100"),
            _ => Result<int>.Success(pageSize)
        };
    }

    public Result<string> ValidateSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Result<string>.ValidationError("Search term cannot be empty");

        return Result<string>.Success(searchTerm)
            .Ensure(s => s.Length >= 2, "Search term must be at least 2 characters")
            .Ensure(s => s.Length <= 100, "Search term cannot exceed 100 characters");
    }

    #endregion

    #region Batch Validation

    /// <summary>
    /// Valida múltiples elementos y retorna todos los errores.
    /// </summary>
    public Result<IEnumerable<T>> ValidateMany<T>(
        IEnumerable<T> items, 
        Func<T, Result<T>> validator)
    {
        var results = items.Select(validator).ToList();
        var failures = results.Where(r => r.IsFailure).ToList();

        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(f => f.Error));
            return Result<IEnumerable<T>>.ValidationError($"Validation failed for {failures.Count} items: {errors}");
        }

        return Result<IEnumerable<T>>.Success(results.Select(r => r.Value!));
    }

    #endregion

    #region Helpers

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex >= email.Length - 1)
            return false;

        var dotIndex = email.LastIndexOf('.');
        return dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
    }

    #endregion
}