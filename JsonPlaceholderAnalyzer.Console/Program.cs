using JsonPlaceholderAnalyzer.Application.Configuration;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.Handlers;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder - DTOs & Pattern Matching Demo  ║");
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
var queryService = serviceProvider.GetRequiredService<QueryService>();
var mappingService = serviceProvider.GetRequiredService<ResponseMappingService>();
var userService = serviceProvider.GetRequiredService<UserService>();
var postService = serviceProvider.GetRequiredService<PostService>();

using var eventHandlers = new ConsoleEventHandlers(notificationService);

// ═══════════════════════════════════════════════════════════════
// DEMO 1: Paginación con Records
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📄 DEMO 1: Paginación con Records");
Console.WriteLine("═══════════════════════════════════════════════════");

var paginationRequest = new PaginationRequest { Page = 1, PageSize = 5 };

// Usando deconstrucción
var (page, pageSize) = paginationRequest;
Console.WriteLine($"\n  PaginationRequest deconstruido: Page={page}, PageSize={pageSize}");

// Query con paginación
var queryRequest = new QueryRequest
{
    Pagination = paginationRequest,
    Sort = new SortRequest { SortBy = "name", Descending = false }
};

Console.WriteLine("\n  Consultando usuarios (página 1, 5 por página, ordenados por nombre)...");
var usersQueryResult = await queryService.QueryUsersAsync(queryRequest);

if (usersQueryResult.IsSuccess)
{
    var response = usersQueryResult.Value!;
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✓ Resultados:");
    Console.WriteLine($"    Total: {response.TotalItems} usuarios");
    Console.WriteLine($"    Páginas: {response.TotalPages}");
    Console.WriteLine($"    Página actual: {response.Page}");
    Console.WriteLine($"    ¿Tiene siguiente?: {response.HasNextPage}");
    Console.ResetColor();
    
    Console.WriteLine("\n  Usuarios en esta página:");
    foreach (var user in response.Items)
    {
        Console.WriteLine($"    - {user.DisplayName} | {user.City} | {user.CompanyName}");
    }
}

// ═══════════════════════════════════════════════════════════════
// DEMO 2: Pattern Matching para clasificación
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔍 DEMO 2: Pattern Matching - Clasificación de entidades");
Console.WriteLine("═══════════════════════════════════════════════════");

var allUsersResult = await userService.GetAllAsync();
var allPostsResult = await postService.GetAllAsync();

if (allUsersResult.IsSuccess && allPostsResult.IsSuccess)
{
    var entities = new List<object>();
    entities.AddRange(allUsersResult.Value!.Take(2));
    entities.AddRange(allPostsResult.Value!.Take(2));
    
    Console.WriteLine("\n  Clasificando entidades con Pattern Matching:");
    
    foreach (var entity in entities)
    {
        var classification = mappingService.ClassifyEntity(entity);
        var priority = mappingService.GetDisplayPriority(entity);
        
        Console.WriteLine($"    [{priority}] {classification}");
    }
}

// ═══════════════════════════════════════════════════════════════
// DEMO 3: Pattern Matching con Property Patterns
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🏷️ DEMO 3: Property Patterns en Posts");
Console.WriteLine("═══════════════════════════════════════════════════");

if (allPostsResult.IsSuccess)
{
    var posts = allPostsResult.Value!.Take(10);
    var postDtos = mappingService.MapPosts(posts, allUsersResult.Value).ToList();
    
    Console.WriteLine("\n  Clasificando posts por longitud (relational patterns):");
    
    foreach (var post in postDtos)
    {
        var formatted = mappingService.FormatForConsole(post);
        
        // Pattern matching con switch expression
        var lengthCategory = post.Length switch
        {
            PostLength.Short => "(Corto)",
            PostLength.Medium => "(Medio)",
            PostLength.Long => "(Largo)",
            PostLength.VeryLong => "(Muy largo)",
            _ => "(Desconocido)"
        };
        
        Console.WriteLine($"    {formatted} {lengthCategory} [{post.WordCount} palabras]");
    }
}

// ═══════════════════════════════════════════════════════════════
// DEMO 4: Records con with-expressions
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📝 DEMO 4: Records y with-expressions");
Console.WriteLine("═══════════════════════════════════════════════════");

var originalRequest = new QueryRequest
{
    Pagination = new PaginationRequest { Page = 1, PageSize = 10 },
    Sort = new SortRequest { SortBy = "title" },
    SearchTerm = "original"
};

Console.WriteLine($"\n  Request original: Page={originalRequest.Pagination.Page}, Search='{originalRequest.SearchTerm}'");

// with-expression para crear copia modificada
var modifiedRequest = originalRequest with
{
    Pagination = originalRequest.Pagination with { Page = 2 },
    SearchTerm = "modificado"
};

Console.WriteLine($"  Request modificado: Page={modifiedRequest.Pagination.Page}, Search='{modifiedRequest.SearchTerm}'");
Console.WriteLine($"  ¿Son iguales?: {originalRequest == modifiedRequest}");

// ═══════════════════════════════════════════════════════════════
// DEMO 5: Query con búsqueda y ordenamiento
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔎 DEMO 5: Query con búsqueda y ordenamiento");
Console.WriteLine("═══════════════════════════════════════════════════");

var searchQuery = new QueryRequest
{
    Pagination = new PaginationRequest { Page = 1, PageSize = 5 },
    Sort = new SortRequest { SortBy = "wordcount", Descending = true },
    SearchTerm = "qui"
};

Console.WriteLine($"\n  Buscando posts con 'qui', ordenados por palabras (desc)...");
var searchResult = await queryService.QueryPostsAsync(searchQuery);

if (searchResult.IsSuccess)
{
    var response = searchResult.Value!;
    var summary = mappingService.GenerateSummary(response);
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n  {summary}");
    Console.ResetColor();
    
    foreach (var post in response.Items)
    {
        Console.WriteLine($"    [{post.WordCount} palabras] {post.Title[..Math.Min(50, post.Title.Length)]}...");
    }
}

// ═══════════════════════════════════════════════════════════════
// DEMO 6: Filtrado de Todos con Pattern Matching
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ DEMO 6: Filtrado de Todos");
Console.WriteLine("═══════════════════════════════════════════════════");

var todoQuery = new QueryRequest
{
    Pagination = new PaginationRequest { Page = 1, PageSize = 10 },
    Sort = new SortRequest { SortBy = "completed" }
};

// Solo pendientes
Console.WriteLine("\n  Todos pendientes:");
var pendingResult = await queryService.QueryTodosAsync(todoQuery, completedFilter: false);
if (pendingResult.IsSuccess)
{
    foreach (var todo in pendingResult.Value!.Items.Take(5))
    {
        var color = mappingService.GetStatusColor(todo);
        Console.ForegroundColor = color;
        Console.WriteLine($"    {todo.StatusEmoji} {todo.Title[..Math.Min(40, todo.Title.Length)]}...");
        Console.ResetColor();
    }
}

// Solo completados
Console.WriteLine("\n  Todos completados:");
var completedResult = await queryService.QueryTodosAsync(todoQuery, completedFilter: true);
if (completedResult.IsSuccess)
{
    foreach (var todo in completedResult.Value!.Items.Take(5))
    {
        var color = mappingService.GetStatusColor(todo);
        Console.ForegroundColor = color;
        Console.WriteLine($"    {todo.StatusEmoji} {todo.Title[..Math.Min(40, todo.Title.Length)]}...");
        Console.ResetColor();
    }
}

Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ All demos completed!");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();