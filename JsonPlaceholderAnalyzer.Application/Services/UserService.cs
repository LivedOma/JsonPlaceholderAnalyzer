using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para gestionar usuarios.
/// 
/// Demuestra:
/// - Primary Constructor heredando de clase base con primary constructor
/// - Override de métodos virtuales
/// - Implementación de métodos abstractos
/// - Métodos adicionales específicos del servicio
/// </summary>
public class UserService(
    IUserRepository repository,
    NotificationService notificationService
) : EntityServiceBase<User, IUserRepository>(repository, notificationService)
{
    /// <summary>
    /// Implementación obligatoria del método abstracto de validación.
    /// </summary>
    protected override Result ValidateEntity(User entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
            return Result.Failure("User name is required");
        
        if (string.IsNullOrWhiteSpace(entity.Email))
            return Result.Failure("User email is required");
        
        if (!entity.Email.Contains('@'))
            return Result.Failure("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(entity.Username))
            return Result.Failure("Username is required");
        
        return Result.Success();
    }

    /// <summary>
    /// Busca un usuario por nombre de usuario.
    /// Método específico de este servicio.
    /// </summary>
    public async Task<Result<User>> GetByUsernameAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result<User>.Failure("Username cannot be empty");
        
        NotificationService.OnLogReceived(
            Domain.Events.LogLevel.Debug, 
            $"Searching user by username: {username}"
        );
        
        return await Repository.GetByUsernameAsync(username, cancellationToken);
    }

    /// <summary>
    /// Busca un usuario por email.
    /// </summary>
    public async Task<Result<User>> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure("Email cannot be empty");
        
        return await Repository.GetByEmailAsync(email, cancellationToken);
    }

    /// <summary>
    /// Obtiene usuarios por nombre de compañía.
    /// </summary>
    public async Task<Result<IEnumerable<User>>> GetByCompanyAsync(
        string companyName, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return Result<IEnumerable<User>>.Failure("Company name cannot be empty");
        
        return await Repository.GetByCompanyNameAsync(companyName, cancellationToken);
    }

    /// <summary>
    /// Obtiene usuarios por ciudad.
    /// </summary>
    public async Task<Result<IEnumerable<User>>> GetByCityAsync(
        string city, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result<IEnumerable<User>>.Failure("City cannot be empty");
        
        return await Repository.GetByCityAsync(city, cancellationToken);
    }

    /// <summary>
    /// Obtiene un resumen de todos los usuarios.
    /// Método que combina múltiples operaciones.
    /// </summary>
    public async Task<Result<UserSummary>> GetUserSummaryAsync(CancellationToken cancellationToken = default)
    {
        var usersResult = await GetAllAsync(cancellationToken);
        
        if (usersResult.IsFailure)
            return Result<UserSummary>.Failure(usersResult.Error ?? "Failed to get users");
        
        var users = usersResult.Value!.ToList();
        
        var summary = new UserSummary
        {
            TotalUsers = users.Count,
            UniqueCompanies = users.Select(u => u.Company.Name).Distinct().Count(),
            UniqueCities = users.Select(u => u.Address.City).Distinct().Count(),
            UsersWithWebsite = users.Count(u => !string.IsNullOrWhiteSpace(u.Website)),
            UsersWithPhone = users.Count(u => !string.IsNullOrWhiteSpace(u.Phone))
        };
        
        return Result<UserSummary>.Success(summary);
    }
}

/// <summary>
/// DTO para resumen de usuarios.
/// </summary>
public record UserSummary
{
    public int TotalUsers { get; init; }
    public int UniqueCompanies { get; init; }
    public int UniqueCities { get; init; }
    public int UsersWithWebsite { get; init; }
    public int UsersWithPhone { get; init; }
}