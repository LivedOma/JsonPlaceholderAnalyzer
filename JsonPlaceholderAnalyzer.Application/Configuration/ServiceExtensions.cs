using JsonPlaceholderAnalyzer.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonPlaceholderAnalyzer.Application.Configuration;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Servicios singleton
        services.AddSingleton<NotificationService>();
        
        // Servicios scoped
        services.AddScoped<DataFilterService>();
        services.AddScoped<ResponseMappingService>();
        services.AddScoped<QueryService>();
        services.AddScoped<AnalyticsService>();
        services.AddScoped<ValidationService>();  // ← NUEVO
        
        // Servicios de entidades
        services.AddScoped<UserService>();
        services.AddScoped<PostService>();
        services.AddScoped<TodoService>();
        services.AddScoped<AlbumService>();
        
        return services;
    }
}