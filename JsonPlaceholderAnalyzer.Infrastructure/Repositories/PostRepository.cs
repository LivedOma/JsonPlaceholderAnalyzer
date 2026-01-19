using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Repositories;

public class PostRepository(
    IApiClient apiClient,
    IMapper<ApiPostDto, Post> mapper,
    IMapper<ApiCommentDto, Comment> commentMapper
) : RepositoryBase<Post, ApiPostDto>(apiClient, mapper, "posts"), IPostRepository
{
    private readonly IMapper<ApiCommentDto, Comment> _commentMapper = commentMapper;

    public async Task<Result<IEnumerable<Post>>> GetByUserIdAsync(
        int userId, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"posts?userId={userId}";
        var result = await ApiClient.GetListAsync<ApiPostDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => 
                Result<IEnumerable<Post>>.Success(result.Value.Select(dto => Mapper.Map(dto))),
            _ => Result<IEnumerable<Post>>.Failure(result.Error ?? "Failed to get posts")
        };
    }

    public async Task<Result<IEnumerable<Post>>> SearchByTitleAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        var allResult = await GetAllAsync(cancellationToken);
        
        if (allResult.IsFailure)
            return Result<IEnumerable<Post>>.Failure(allResult.Error ?? "Failed to get posts");
        
        var posts = allResult.Value?
            .Where(p => p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        
        return Result<IEnumerable<Post>>.Success(posts);
    }

    public async Task<Result<IEnumerable<Comment>>> GetCommentsForPostAsync(
        int postId, 
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"posts/{postId}/comments";
        var result = await ApiClient.GetListAsync<ApiCommentDto>(endpoint, cancellationToken);
        
        return result switch
        {
            { IsSuccess: true, Value: not null } => 
                Result<IEnumerable<Comment>>.Success(result.Value.Select(dto => _commentMapper.Map(dto))),
            _ => Result<IEnumerable<Comment>>.Failure(result.Error ?? "Failed to get comments")
        };
    }
}