using JsonPlaceholderAnalyzer.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonPlaceholderAnalyzer.Application.Configuration;

/// <summary>
/// Extensiones para registrar servicios de la capa Application.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // NotificationService debe ser Singleton para que todos compartan la misma instancia
        services.AddSingleton<NotificationService>();
        
        // DataFilterService 
        services.AddScoped<DataFilterService>();
        
        // Servicios de entidades
        services.AddScoped<UserService>();
        services.AddScoped<PostService>();
        services.AddScoped<TodoService>();
        services.AddScoped<AlbumService>();
        
        return services;
    }
}