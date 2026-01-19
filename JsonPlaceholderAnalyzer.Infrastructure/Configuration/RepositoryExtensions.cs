using Microsoft.Extensions.DependencyInjection;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.Mappers;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;

namespace JsonPlaceholderAnalyzer.Infrastructure.Configuration;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Registrar Mappers
        services.AddSingleton<IMapper<ApiUserDto, User>, UserMapper>();
        services.AddSingleton<IMapper<ApiPostDto, Post>, PostMapper>();
        services.AddSingleton<IMapper<ApiCommentDto, Comment>, CommentMapper>();
        services.AddSingleton<IMapper<ApiAlbumDto, Album>, AlbumMapper>();
        services.AddSingleton<IMapper<ApiPhotoDto, Photo>, PhotoMapper>();
        services.AddSingleton<IMapper<ApiTodoDto, Todo>, TodoMapper>();
        
        // Registrar Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        
        return services;
    }
}