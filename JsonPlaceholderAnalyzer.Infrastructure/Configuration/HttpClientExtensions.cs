using Microsoft.Extensions.DependencyInjection;
using JsonPlaceholderAnalyzer.Domain.Interfaces;
using JsonPlaceholderAnalyzer.Infrastructure.ApiClient;

namespace JsonPlaceholderAnalyzer.Infrastructure.Configuration;

/// <summary>
/// Extensiones para configurar el cliente HTTP en el contenedor de DI.
/// Demuestra: Extension methods, configuración de HttpClient.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Agrega y configura el cliente de JSONPlaceholder API.
    /// </summary>
    public static IServiceCollection AddJsonPlaceholderApiClient(
        this IServiceCollection services,
        ApiSettings? settings = null)
    {
        // Operador ??= no aplica aquí, pero ?? sí
        settings ??= ApiSettings.Default;
        
        // Registrar settings como singleton
        services.AddSingleton(settings);
        
        // Configurar HttpClient con timeout
        services.AddHttpClient<JsonPlaceholderApiClient>(client =>
        {
            client.BaseAddress = new Uri(settings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "JsonPlaceholderAnalyzer/1.0");
        });
        
        // Registrar el cliente básico
        services.AddScoped<JsonPlaceholderApiClient>();
        
        // Registrar el cliente con reintentos como la implementación de IApiClient
        services.AddScoped<IApiClient>(provider =>
        {
            var innerClient = provider.GetRequiredService<JsonPlaceholderApiClient>();
            var apiSettings = provider.GetRequiredService<ApiSettings>();
            return new RetryingApiClient(innerClient, apiSettings);
        });
        
        return services;
    }
}