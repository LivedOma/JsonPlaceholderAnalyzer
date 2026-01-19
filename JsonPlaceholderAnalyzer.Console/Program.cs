using JsonPlaceholderAnalyzer.Application.Configuration;
using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.Handlers;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Interfaces;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder - Events & Delegates Demo       ║");
Console.WriteLine("╚═══════════════════════════════════════════════════╝");
Console.WriteLine();

// Configurar servicios
var services = new ServiceCollection();
services.AddJsonPlaceholderApiClient();
services.AddRepositories();
services.AddApplicationServices();

var serviceProvider = services.BuildServiceProvider();

// Obtener servicios
var notificationService = serviceProvider.GetRequiredService<NotificationService>();
var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
var postRepo = serviceProvider.GetRequiredService<IPostRepository>();
var todoRepo = serviceProvider.GetRequiredService<ITodoRepository>();
var filterService = serviceProvider.GetRequiredService<DataFilterService>();

// Crear manejador de eventos de consola
using var eventHandlers = new ConsoleEventHandlers(notificationService);
eventHandlers.VerboseMode = true; // Activar modo verbose para ver más detalles

// ═══════════════════════════════════════════════════════════════
// DEMO 1: Eventos de notificación simple
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📢 DEMO 1: Notificaciones simples");
Console.WriteLine("═══════════════════════════════════════════════════");

notificationService.OnNotification("¡Bienvenido al sistema de eventos!");
notificationService.SendMessage("Este es un mensaje simple");
notificationService.SendColoredMessage("Este mensaje es colorido", ConsoleColor.Magenta);

// ═══════════════════════════════════════════════════════════════
// DEMO 2: Eventos de API
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🌐 DEMO 2: Eventos de API (cargando usuarios)");
Console.WriteLine("═══════════════════════════════════════════════════");

var usersResult = await userRepo.GetAllAsync();
if (usersResult.IsSuccess)
{
    Console.WriteLine($"\n  ✓ Loaded {usersResult.Value?.Count() ?? 0} users");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 3: Eventos de entidades (CRUD simulado)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📝 DEMO 3: Eventos de entidades");
Console.WriteLine("═══════════════════════════════════════════════════");

// Simular creación de entidad
var newTodo = new Todo
{
    Id = 0,
    UserId = 1,
    Title = "Learn about delegates and events",
    Completed = false
};

notificationService.OnEntityCreated(newTodo, 201);

// Simular actualización
var updatedTodo = new Todo
{
    Id = 201,
    UserId = 1,
    Title = "Learn about delegates and events",
    Completed = true
};

notificationService.OnEntityUpdated(updatedTodo, newTodo);

// Simular eliminación
notificationService.OnEntityDeleted(201, "Todo");

// ═══════════════════════════════════════════════════════════════
// DEMO 4: Uso de Func<T> y predicados para filtrado
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔍 DEMO 4: Filtrado con Func<T, bool>");
Console.WriteLine("═══════════════════════════════════════════════════");

var todosResult = await todoRepo.GetAllAsync();
if (todosResult.IsSuccess)
{
    var todos = todosResult.Value!.ToList();
    
    Console.WriteLine("\n  Filtering completed todos using Func<Todo, bool>...");
    
    // Usando Func<Todo, bool> como predicado
    Func<Todo, bool> isCompleted = todo => todo.Completed;
    
    var completedTodos = filterService.Filter(
        todos, 
        isCompleted,
        onItemFiltered: todo => { /* callback opcional por cada item filtrado */ }
    );
    
    Console.WriteLine($"\n  Found {completedTodos.Count()} completed todos");
    
    // Filtrar con lambda directa
    Console.WriteLine("\n  Filtering todos with 'et' in title...");
    var todosWithEt = filterService.Filter(
        todos,
        t => t.Title.Contains("et", StringComparison.OrdinalIgnoreCase)
    );
    Console.WriteLine($"  Found {todosWithEt.Count()} todos with 'et' in title");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 5: Transformación con Func<TSource, TResult>
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔄 DEMO 5: Transformación con Func<T, TResult>");
Console.WriteLine("═══════════════════════════════════════════════════");

if (usersResult.IsSuccess)
{
    var users = usersResult.Value!.ToList();
    
    // Transformar usuarios a un formato simple
    Func<User, string> toDisplayString = user => $"{user.Name} ({user.Email})";
    
    var displayStrings = filterService.Transform(users, toDisplayString);
    
    Console.WriteLine("\n  Users transformed to display strings:");
    foreach (var display in displayStrings.Take(3))
    {
        Console.WriteLine($"    - {display}");
    }
}

// ═══════════════════════════════════════════════════════════════
// DEMO 6: Agrupación con Func<T, TKey>
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📊 DEMO 6: Agrupación con Func<T, TKey>");
Console.WriteLine("═══════════════════════════════════════════════════");

if (todosResult.IsSuccess)
{
    var todos = todosResult.Value!.ToList();
    
    // Agrupar por UserId
    Func<Todo, int> byUserId = todo => todo.UserId;
    
    var groupedByUser = filterService.GroupBy(todos, byUserId);
    
    Console.WriteLine("\n  Todos grouped by UserId:");
    foreach (var group in groupedByUser.Take(3))
    {
        Console.WriteLine($"    User {group.Key}: {group.Value.Count} todos");
    }
    
    // Agrupar por estado de completado
    var groupedByStatus = filterService.GroupBy(todos, t => t.Completed);
    Console.WriteLine($"\n  Completed: {groupedByStatus.GetValueOrDefault(true)?.Count ?? 0}");
    Console.WriteLine($"  Pending: {groupedByStatus.GetValueOrDefault(false)?.Count ?? 0}");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 7: ForEach con Action<T>
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔁 DEMO 7: ForEach con Action<T>");
Console.WriteLine("═══════════════════════════════════════════════════");

if (usersResult.IsSuccess)
{
    var users = usersResult.Value!.Take(3);
    
    // Action<User> - no retorna nada, solo ejecuta
    Action<User> printUser = user =>
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    👤 {user.Name}");
        Console.ResetColor();
    };
    
    Console.WriteLine("\n  First 3 users:");
    filterService.ForEach(users, printUser);
    
    // Action<User, int> - con índice
    Console.WriteLine("\n  With index:");
    filterService.ForEachWithIndex(users, (user, index) =>
    {
        Console.WriteLine($"    [{index + 1}] {user.Username}");
    });
}

// ═══════════════════════════════════════════════════════════════
// DEMO 8: Progreso con delegado personalizado
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("⏳ DEMO 8: Barra de progreso con ProgressHandler");
Console.WriteLine("═══════════════════════════════════════════════════\n");

for (int i = 1; i <= 20; i++)
{
    notificationService.OnProgressUpdate(i, 20, "Processing items");
    await Task.Delay(100); // Simular trabajo
}

Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ All demos completed!");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();