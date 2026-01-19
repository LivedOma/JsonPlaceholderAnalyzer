using JsonPlaceholderAnalyzer.Application.Configuration;
using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.UI;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configurar servicios
var services = new ServiceCollection();
services.AddJsonPlaceholderApiClient();
services.AddRepositories();
services.AddApplicationServices();

// Registrar la aplicación de consola
services.AddSingleton<ConsoleApp>();

var serviceProvider = services.BuildServiceProvider();

// Ejecutar la aplicación
var app = serviceProvider.GetRequiredService<ConsoleApp>();
await app.RunAsync();