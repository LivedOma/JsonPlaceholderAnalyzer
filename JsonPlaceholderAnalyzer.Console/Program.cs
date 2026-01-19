using JsonPlaceholderAnalyzer.Application.Configuration;
using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.Handlers;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder - LINQ Analytics Demo           ║");
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
var analyticsService = serviceProvider.GetRequiredService<AnalyticsService>();

using var eventHandlers = new ConsoleEventHandlers(notificationService);

// Cargar datos
Console.WriteLine("Cargando datos...\n");
var loadResult = await analyticsService.LoadDataAsync();
if (loadResult.IsFailure)
{
    Console.WriteLine($"Error: {loadResult.Error}");
    return;
}

// ═══════════════════════════════════════════════════════════════
// DEMO 1: Operadores de Proyección (Select, SelectMany)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📊 DEMO 1: SELECT & SELECTMANY");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Emails de usuarios (Select simple):");
foreach (var email in analyticsService.GetUserEmails().Take(5))
{
    Console.WriteLine($"    📧 {email}");
}

Console.WriteLine("\n  Resumen de usuarios (Select con proyección):");
foreach (var summary in analyticsService.GetUserSummaries().Take(3))
{
    Console.WriteLine($"    👤 {summary.FullName}");
    Console.WriteLine($"       Posts: {summary.PostCount}, Todos: {summary.TodoCount} ({summary.TodoCompletionRate:F0}% completados)");
}

Console.WriteLine("\n  Palabras de títulos (SelectMany - aplanado):");
var words = analyticsService.GetAllWordsFromTitles().Take(15).ToList();
Console.WriteLine($"    Primeras 15 palabras: {string.Join(", ", words)}");

// ═══════════════════════════════════════════════════════════════
// DEMO 2: Operadores de Filtrado (Where, Distinct)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔍 DEMO 2: WHERE & DISTINCT");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Ciudades únicas (Distinct):");
foreach (var city in analyticsService.GetUniqueCities())
{
    Console.WriteLine($"    🏙️ {city}");
}

Console.WriteLine("\n  Un usuario por compañía (DistinctBy):");
foreach (var user in analyticsService.GetOneUserPerCompany().Take(5))
{
    Console.WriteLine($"    🏢 {user.Company.Name}: {user.Name}");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 3: Operadores de Ordenamiento (OrderBy, ThenBy)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📈 DEMO 3: ORDERBY & THENBY");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Posts ordenados por palabras (desc):");
foreach (var post in analyticsService.GetPostsByWordCountDesc().Take(5))
{
    Console.WriteLine($"    [{post.WordCount} palabras] {post.ShortTitle}");
}

Console.WriteLine("\n  Usuarios ordenados por ciudad, luego compañía:");
foreach (var user in analyticsService.GetUsersSortedByCityThenName().Take(5))
{
    Console.WriteLine($"    📍 {user.Address.City} | 🏢 {user.Company.Name} | {user.Name}");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 4: Operadores de Agrupación (GroupBy)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📁 DEMO 4: GROUPBY");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Estadísticas de posts por usuario:");
foreach (var stats in analyticsService.GetPostStatsByUser().Take(5))
{
    Console.WriteLine($"    👤 {stats.UserName}:");
    Console.WriteLine($"       Posts: {stats.PostCount}, Total palabras: {stats.TotalWords}, Promedio: {stats.AverageWords:F1}");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 5: Operadores de Agregación
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔢 DEMO 5: COUNT, SUM, AVERAGE, MIN, MAX, AGGREGATE");
Console.WriteLine("═══════════════════════════════════════════════════");

var counts = analyticsService.GetBasicCounts();
Console.WriteLine($"\n  Conteos básicos:");
Console.WriteLine($"    Usuarios: {counts.TotalUsers}");
Console.WriteLine($"    Posts: {counts.TotalPosts}");
Console.WriteLine($"    Todos: {counts.TotalTodos} (✓{counts.CompletedTodos} / ○{counts.PendingTodos})");

var extremes = analyticsService.GetWordCountExtremes();
Console.WriteLine($"\n  Extremos de palabras:");
Console.WriteLine($"    Mínimo: {extremes.Min} palabras (Post #{extremes.Shortest?.Id})");
Console.WriteLine($"    Máximo: {extremes.Max} palabras (Post #{extremes.Longest?.Id})");
Console.WriteLine($"    Promedio: {analyticsService.GetAverageWordsPerPost():F1} palabras");

var wordStats = analyticsService.GetWordStatistics();
Console.WriteLine($"\n  Estadísticas con Aggregate:");
Console.WriteLine($"    Total posts: {wordStats.TotalPosts}, Total palabras: {wordStats.TotalWords}");

Console.WriteLine($"\n  Todos los usernames concatenados (Aggregate):");
Console.WriteLine($"    {analyticsService.ConcatenateAllUsernames()}");

// ═══════════════════════════════════════════════════════════════
// DEMO 6: Operadores de Conjunto (Union, Intersect, Except)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("⚙️ DEMO 6: UNION, INTERSECT, EXCEPT");
Console.WriteLine("═══════════════════════════════════════════════════");

var allUserIds = analyticsService.GetAllUserIdsWithPostsOrTodos().ToList();
Console.WriteLine($"\n  IDs con posts O todos (Union): {string.Join(", ", allUserIds)}");

var intersectIds = analyticsService.GetUserIdsWithBothPostsAndCompletedTodos().ToList();
Console.WriteLine($"  IDs con posts Y todos completados (Intersect): {string.Join(", ", intersectIds)}");

// ═══════════════════════════════════════════════════════════════
// DEMO 7: Operadores de Cuantificación (Any, All, Contains)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("❓ DEMO 7: ANY, ALL, CONTAINS");
Console.WriteLine("═══════════════════════════════════════════════════");

var quant = analyticsService.GetQuantificationResults();
Console.WriteLine($"\n  Any:");
Console.WriteLine($"    ¿Hay usuarios? {quant.HasUsers}");
Console.WriteLine($"    ¿Hay usuarios de Gwenborough? {quant.HasUsersFromGwenborough}");
Console.WriteLine($"    ¿Hay posts largos (>100 palabras)? {quant.HasLongPosts}");

Console.WriteLine($"\n  All:");
Console.WriteLine($"    ¿Todos los usuarios tienen email? {quant.AllUsersHaveEmail}");
Console.WriteLine($"    ¿Todos los posts tienen contenido? {quant.AllPostsHaveContent}");

Console.WriteLine($"\n  Contains:");
Console.WriteLine($"    ¿Contiene userId=1? {quant.ContainsUserId1}");
Console.WriteLine($"    ¿Contiene userId=999? {quant.ContainsUserId999}");

// ═══════════════════════════════════════════════════════════════
// DEMO 8: Operadores de Partición (Take, Skip, Chunk)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📄 DEMO 8: TAKE, SKIP, CHUNK");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Top 3 posts más largos (Take + OrderBy):");
foreach (var post in analyticsService.GetTopPosts(3))
{
    Console.WriteLine($"    [{post.WordCount} palabras] {post.ShortTitle}");
}

Console.WriteLine("\n  Página 2 de posts (5 por página):");
foreach (var post in analyticsService.GetPostsPage(2, 5))
{
    Console.WriteLine($"    #{post.Id}: {post.ShortTitle}");
}

Console.WriteLine("\n  Posts en chunks de 25:");
var chunks = analyticsService.GetPostsInChunks(25).ToList();
Console.WriteLine($"    Total de chunks: {chunks.Count}");
for (int i = 0; i < Math.Min(3, chunks.Count); i++)
{
    Console.WriteLine($"    Chunk {i + 1}: {chunks[i].Length} posts");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 9: Operadores de Unión (Join, GroupJoin, Zip)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔗 DEMO 9: JOIN, GROUPJOIN, ZIP");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n  Posts con autores (Join):");
foreach (var item in analyticsService.GetPostsWithAuthors().Take(5))
{
    Console.WriteLine($"    📝 \"{item.PostTitle[..Math.Min(30, item.PostTitle.Length)]}...\" por {item.AuthorName}");
}

Console.WriteLine("\n  Usuarios con sus posts (GroupJoin):");
foreach (var item in analyticsService.GetUsersWithTheirPosts().Take(3))
{
    Console.WriteLine($"    👤 {item.UserName}: {item.PostCount} posts");
}

Console.WriteLine("\n  Zip de usuarios y posts:");
foreach (var item in analyticsService.ZipUsersWithPosts().Take(5))
{
    Console.WriteLine($"    {item}");
}

// ═══════════════════════════════════════════════════════════════
// DEMO 10: Análisis Comprehensivo
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📊 DEMO 10: ANÁLISIS COMPLETO (Combinación de LINQ)");
Console.WriteLine("═══════════════════════════════════════════════════");

var analysis = analyticsService.GetComprehensiveAnalysis();

Console.WriteLine("\n  🏆 TOP 5 Usuarios más activos:");
foreach (var user in analysis.MostActiveUsers)
{
    Console.WriteLine($"    {user.UserName}: {user.PostCount} posts, {user.CompletedTodos} todos completados ({user.TodoCompletionRate:F0}%)");
}

Console.WriteLine("\n  📝 TOP 10 Palabras más frecuentes en títulos:");
foreach (var word in analysis.TopWords)
{
    Console.WriteLine($"    \"{word.Word}\": {word.Count} veces");
}

Console.WriteLine("\n  📋 Distribución de todos por usuario:");
foreach (var dist in analysis.TodoDistribution.Take(5))
{
    Console.WriteLine($"    {dist.UserName}: ✓{dist.Completed} / ○{dist.Pending} (🔥{dist.HighPriority} urgentes)");
}

Console.WriteLine($"\n  📈 Resumen:");
Console.WriteLine($"    Posts analizados: {analysis.TotalAnalyzedPosts}");
Console.WriteLine($"    Todos analizados: {analysis.TotalAnalyzedTodos}");
Console.WriteLine($"    Tasa global de completado: {analysis.OverallCompletionRate:F1}%");

Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ Todas las demos de LINQ completadas!");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();