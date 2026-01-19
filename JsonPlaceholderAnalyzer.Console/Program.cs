using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using JsonPlaceholderAnalyzer.Infrastructure.ApiClient;
using JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;
using JsonPlaceholderAnalyzer.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder API Client Test                 ║");
Console.WriteLine("╚═══════════════════════════════════════════════════╝");
Console.WriteLine();

// Configurar servicios
var services = new ServiceCollection();
services.AddJsonPlaceholderApiClient();

var serviceProvider = services.BuildServiceProvider();

// Obtener el cliente
var apiClient = serviceProvider.GetRequiredService<IApiClient>();

// Prueba 1: Obtener todos los usuarios
Console.WriteLine("📋 Fetching users...");
var usersResult = await apiClient.GetListAsync<ApiUserDto>("users");

if (usersResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✓ Success! Found {usersResult.Value?.Count() ?? 0} users");
    Console.ResetColor();
    
    foreach (var user in usersResult.Value?.Take(3) ?? [])
    {
        Console.WriteLine($"  - {user.Id}: {user.Name} ({user.Email})");
    }
    Console.WriteLine("  ...");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✗ Error: {usersResult.Error}");
    Console.ResetColor();
}

Console.WriteLine();

// Prueba 2: Obtener un post específico
Console.WriteLine("📝 Fetching post #1...");
var postResult = await apiClient.GetAsync<ApiPostDto>("posts/1");

if (postResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✓ Success!");
    Console.ResetColor();
    Console.WriteLine($"  Title: {postResult.Value?.Title}");
    Console.WriteLine($"  Body: {postResult.Value?.Body?[..50]}...");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"✗ Error: {postResult.Error}");
    Console.ResetColor();
}

Console.WriteLine();

// Prueba 3: Obtener todos los todos
Console.WriteLine("✅ Fetching todos...");
var todosResult = await apiClient.GetListAsync<ApiTodoDto>("todos");

if (todosResult.IsSuccess)
{
    var todos = todosResult.Value?.ToList() ?? [];
    var completed = todos.Count(t => t.Completed);
    var pending = todos.Count - completed;
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✓ Success! Found {todos.Count} todos");
    Console.ResetColor();
    Console.WriteLine($"  - Completed: {completed}");
    Console.WriteLine($"  - Pending: {pending}");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();