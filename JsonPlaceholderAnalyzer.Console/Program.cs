using JsonPlaceholderAnalyzer.Application.Configuration;
using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.Handlers;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder - Application Services Demo     ║");
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
var userService = serviceProvider.GetRequiredService<UserService>();
var postService = serviceProvider.GetRequiredService<PostService>();
var todoService = serviceProvider.GetRequiredService<TodoService>();
var albumService = serviceProvider.GetRequiredService<AlbumService>();

// Crear manejador de eventos
using var eventHandlers = new ConsoleEventHandlers(notificationService);

// ═══════════════════════════════════════════════════════════════
// DEMO 1: UserService
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("👤 USER SERVICE DEMO");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n1. Getting user summary...");
var userSummaryResult = await userService.GetUserSummaryAsync();
if (userSummaryResult.IsSuccess)
{
    var summary = userSummaryResult.Value!;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Total Users: {summary.TotalUsers}");
    Console.WriteLine($"   ✓ Unique Companies: {summary.UniqueCompanies}");
    Console.WriteLine($"   ✓ Unique Cities: {summary.UniqueCities}");
    Console.WriteLine($"   ✓ Users with Website: {summary.UsersWithWebsite}");
    Console.WriteLine($"   ✓ Users with Phone: {summary.UsersWithPhone}");
    Console.ResetColor();
}

Console.WriteLine("\n2. Finding user by username 'Bret'...");
var bretResult = await userService.GetByUsernameAsync("Bret");
if (bretResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"   ✓ Found: {bretResult.Value!.Name}");
    Console.WriteLine($"     Email: {bretResult.Value.Email}");
    Console.WriteLine($"     Company: {bretResult.Value.Company.Name}");
    Console.ResetColor();
}

// ═══════════════════════════════════════════════════════════════
// DEMO 2: PostService
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📝 POST SERVICE DEMO");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n1. Getting post statistics...");
var postStatsResult = await postService.GetStatisticsAsync();
if (postStatsResult.IsSuccess)
{
    var stats = postStatsResult.Value!;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Total Posts: {stats.TotalPosts}");
    Console.WriteLine($"   ✓ Total Words: {stats.TotalWords:N0}");
    Console.WriteLine($"   ✓ Average Words/Post: {stats.AverageWordsPerPost:F1}");
    Console.WriteLine($"   ✓ Longest Post: #{stats.LongestPost?.Id} ({stats.LongestPost?.WordCount} words)");
    Console.WriteLine($"   ✓ Shortest Post: #{stats.ShortestPost?.Id} ({stats.ShortestPost?.WordCount} words)");
    Console.ResetColor();
}

Console.WriteLine("\n2. Getting post #1 with comments...");
var postWithCommentsResult = await postService.GetWithCommentsAsync(1);
if (postWithCommentsResult.IsSuccess)
{
    var data = postWithCommentsResult.Value!;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"   ✓ Post: {data.Post.ShortTitle}");
    Console.WriteLine($"   ✓ Comments: {data.CommentCount}");
    
    foreach (var comment in data.Comments.Take(2))
    {
        Console.WriteLine($"     - {comment.ShortName} ({comment.Email})");
    }
    Console.ResetColor();
}

// ═══════════════════════════════════════════════════════════════
// DEMO 3: TodoService
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ TODO SERVICE DEMO");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n1. Getting todo statistics...");
var todoStatsResult = await todoService.GetStatisticsAsync();
if (todoStatsResult.IsSuccess)
{
    var stats = todoStatsResult.Value!;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Total Todos: {stats.Total}");
    Console.WriteLine($"   ✓ Completed: {stats.Completed} ({stats.CompletionRate:F1}%)");
    Console.WriteLine($"   ✓ Pending: {stats.Pending}");
    Console.WriteLine($"   ✓ High Priority: {stats.HighPriority}");
    Console.WriteLine($"   ✓ Medium Priority: {stats.MediumPriority}");
    Console.WriteLine($"   ✓ Low Priority: {stats.LowPriority}");
    Console.ResetColor();
    
    Console.WriteLine("\n   Top 3 users by todo count:");
    foreach (var (userId, userStats) in stats.TodosPerUser.OrderByDescending(x => x.Value.Total).Take(3))
    {
        Console.WriteLine($"     User #{userId}: {userStats.Total} todos ({userStats.CompletionRate:F0}% complete)");
    }
}

Console.WriteLine("\n2. Toggling todo #1...");
var toggleResult = await todoService.ToggleCompletedAsync(1);
if (toggleResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"   ✓ Todo #{toggleResult.Value!.Id}: {toggleResult.Value.Status}");
    Console.ResetColor();
}

// ═══════════════════════════════════════════════════════════════
// DEMO 4: AlbumService
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📸 ALBUM SERVICE DEMO");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n1. Getting album statistics...");
var albumStatsResult = await albumService.GetStatisticsAsync();
if (albumStatsResult.IsSuccess)
{
    var stats = albumStatsResult.Value!;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Total Albums: {stats.TotalAlbums}");
    Console.WriteLine($"   ✓ Estimated Total Photos: {stats.EstimatedTotalPhotos:N0}");
    Console.WriteLine($"   ✓ Average Albums/User: {stats.AverageAlbumsPerUser:F1}");
    Console.ResetColor();
}

Console.WriteLine("\n2. Getting album #1 with photos...");
var albumWithPhotosResult = await albumService.GetWithPhotosAsync(1);
if (albumWithPhotosResult.IsSuccess)
{
    var album = albumWithPhotosResult.Value!;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"   ✓ Album: {album.Title}");
    Console.WriteLine($"   ✓ Photos: {album.PhotoCount}");
    
    foreach (var photo in album.Photos.Take(3))
    {
        Console.WriteLine($"     - {photo.Title[..Math.Min(40, photo.Title.Length)]}...");
    }
    Console.ResetColor();
}

// ═══════════════════════════════════════════════════════════════
// DEMO 5: Create/Update/Delete (Simulated)
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("🔄 CRUD OPERATIONS DEMO (Simulated)");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\n1. Creating a new todo...");
var newTodo = new JsonPlaceholderAnalyzer.Domain.Entities.Todo
{
    Id = 0,
    UserId = 1,
    Title = "Learn C# services pattern",
    Completed = false
};

var createResult = await todoService.CreateAsync(newTodo);
if (createResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Created todo #{createResult.Value!.Id}");
    Console.ResetColor();
}

Console.WriteLine("\n2. Updating the todo...");
var todoToUpdate = createResult.Value!;
var updatedTodo = new JsonPlaceholderAnalyzer.Domain.Entities.Todo
{
    Id = todoToUpdate.Id,
    UserId = todoToUpdate.UserId,
    Title = "Learn C# services pattern - Updated!",
    Completed = true
};

var updateResult = await todoService.UpdateAsync(updatedTodo);
if (updateResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"   ✓ Updated todo #{updateResult.Value!.Id}: {updateResult.Value.Status}");
    Console.ResetColor();
}

Console.WriteLine("\n3. Deleting the todo...");
var deleteResult = await todoService.DeleteAsync(updatedTodo.Id);
if (deleteResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"   ✓ Deleted todo #{updatedTodo.Id}");
    Console.ResetColor();
}

Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ All service demos completed!");
Console.WriteLine("═══════════════════════════════════════════════════");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();