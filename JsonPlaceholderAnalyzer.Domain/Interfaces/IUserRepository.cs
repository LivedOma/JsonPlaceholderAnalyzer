using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Domain.Interfaces;

/// <summary>
/// Repositorio específico para usuarios con operaciones adicionales.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<Result<User>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<User>>> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<User>>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
}