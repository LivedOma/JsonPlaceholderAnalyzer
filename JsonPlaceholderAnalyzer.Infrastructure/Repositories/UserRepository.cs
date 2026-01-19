using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Repositories;

public class UserRepository(
    IApiClient apiClient,
    IMapper<ApiUserDto, User> mapper
) : RepositoryBase<User, ApiUserDto>(apiClient, mapper, "users"), IUserRepository
{
    public async Task<Result<User>> GetByUsernameAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<User>.Failure(allResult.Error ?? "Failed to get users");
        
        var user = allResult.Value?
            .FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        return user is not null
            ? Result<User>.Success(user)
            : Result<User>.Failure($"User with username '{username}' not found");
    }

    public async Task<Result<User>> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<User>.Failure(allResult.Error ?? "Failed to get users");
        
        var user = allResult.Value?
            .FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        return user is not null
            ? Result<User>.Success(user)
            : Result<User>.Failure($"User with email '{email}' not found");
    }

    public async Task<Result<IEnumerable<User>>> GetByCompanyNameAsync(
        string companyName, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<IEnumerable<User>>.Failure(allResult.Error ?? "Failed to get users");
        
        var users = allResult.Value?
            .Where(u => u.Company.Name.Contains(companyName, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        
        return Result<IEnumerable<User>>.Success(users);
    }

    public async Task<Result<IEnumerable<User>>> GetByCityAsync(
        string city, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<IEnumerable<User>>.Failure(allResult.Error ?? "Failed to get users");
        
        var users = allResult.Value?
            .Where(u => u.Address.City.Equals(city, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        
        return Result<IEnumerable<User>>.Success(users);
    }
}