using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio para gestionar posts.
/// </summary>
public class PostService(
    IPostRepository repository,
    NotificationService notificationService,
    IUserRepository userRepository
) : EntityServiceBase<Post, IPostRepository>(repository, notificationService)
{
    private readonly IUserRepository _userRepository = userRepository;

    protected override Result ValidateEntity(Post entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Title))
            return Result.Failure("Post title is required");
        
        if (entity.Title.Length > 200)
            return Result.Failure("Post title cannot exceed 200 characters");
        
        if (string.IsNullOrWhiteSpace(entity.Body))
            return Result.Failure("Post body is required");
        
        if (entity.UserId <= 0)
            return Result.Failure("Valid user ID is required");
        
        return Result.Success();
    }

    /// <summary>
    /// Override para agregar validación de usuario existente.
    /// </summary>
    protected override async Task<Result> BeforeSaveAsync(Post entity, CancellationToken cancellationToken)
    {
        // Verificar que el usuario existe
        var userExists = await _userRepository.ExistsAsync(entity.UserId, cancellationToken);
        
        if (userExists.IsFailure || !userExists.Value)
        {
            return Result.Failure($"User with ID {entity.UserId} does not exist");
        }
        
        return Result.Success();
    }

    /// <summary>
    /// Obtiene posts de un usuario específico.
    /// </summary>
    public async Task<Result<IEnumerable<Post>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Result<IEnumerable<Post>>.Failure("Invalid user ID");
        
        return await Repository.GetByUserIdAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Busca posts por título.
    /// </summary>
    public async Task<Result<IEnumerable<Post>>> SearchByTitleAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Result<IEnumerable<Post>>.Failure("Search term cannot be empty");
        
        return await Repository.SearchByTitleAsync(searchTerm, cancellationToken);
    }

    /// <summary>
    /// Obtiene comentarios de un post.
    /// </summary>
    public async Task<Result<IEnumerable<Comment>>> GetCommentsAsync(
        int postId, 
        CancellationToken cancellationToken = default)
    {
        if (postId <= 0)
            return Result<IEnumerable<Comment>>.Failure("Invalid post ID");
        
        return await Repository.GetCommentsForPostAsync(postId, cancellationToken);
    }

    /// <summary>
    /// Obtiene un post con sus comentarios.
    /// </summary>
    public async Task<Result<PostWithComments>> GetWithCommentsAsync(
        int postId, 
        CancellationToken cancellationToken = default)
    {
        var postResult = await GetByIdAsync(postId, cancellationToken);
        if (postResult.IsFailure)
            return Result<PostWithComments>.Failure(postResult.Error ?? "Post not found");
        
        var commentsResult = await GetCommentsAsync(postId, cancellationToken);
        
        var postWithComments = new PostWithComments
        {
            Post = postResult.Value!,
            Comments = commentsResult.IsSuccess 
                ? commentsResult.Value!.ToList() 
                : new List<Comment>()
        };
        
        return Result<PostWithComments>.Success(postWithComments);
    }

    /// <summary>
    /// Obtiene estadísticas de posts.
    /// </summary>
    public async Task<Result<PostStatistics>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var postsResult = await GetAllAsync(cancellationToken);
        
        if (postsResult.IsFailure)
            return Result<PostStatistics>.Failure(postsResult.Error ?? "Failed to get posts");
        
        var posts = postsResult.Value!.ToList();
        
        var stats = new PostStatistics
        {
            TotalPosts = posts.Count,
            TotalWords = posts.Sum(p => p.WordCount),
            AverageWordsPerPost = posts.Count > 0 ? posts.Average(p => p.WordCount) : 0,
            LongestPost = posts.OrderByDescending(p => p.WordCount).FirstOrDefault(),
            ShortestPost = posts.OrderBy(p => p.WordCount).FirstOrDefault(),
            PostsPerUser = posts.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.Count())
        };
        
        return Result<PostStatistics>.Success(stats);
    }
}

/// <summary>
/// DTO para post con comentarios.
/// </summary>
public record PostWithComments
{
    public required Post Post { get; init; }
    public List<Comment> Comments { get; init; } = new();
    public int CommentCount => Comments.Count;
}

/// <summary>
/// DTO para estadísticas de posts.
/// </summary>
public record PostStatistics
{
    public int TotalPosts { get; init; }
    public int TotalWords { get; init; }
    public double AverageWordsPerPost { get; init; }
    public Post? LongestPost { get; init; }
    public Post? ShortestPost { get; init; }
    public Dictionary<int, int> PostsPerUser { get; init; } = new();
}