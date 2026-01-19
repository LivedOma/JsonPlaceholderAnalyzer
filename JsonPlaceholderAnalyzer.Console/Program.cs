using JsonPlaceholderAnalyzer.Domain.Interfaces;
using JsonPlaceholderAnalyzer.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║   JSONPlaceholder Repository Test                 ║");
Console.WriteLine("╚═══════════════════════════════════════════════════╝");
Console.WriteLine();

// Configurar servicios
var services = new ServiceCollection();
services.AddJsonPlaceholderApiClient();
services.AddRepositories();

var serviceProvider = services.BuildServiceProvider();

// Prueba de UserRepository
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("📋 USER REPOSITORY TEST");
Console.WriteLine("═══════════════════════════════════════════════════");

var userRepo = serviceProvider.GetRequiredService<IUserRepository>();

// Obtener todos los usuarios
Console.WriteLine("\n1. Getting all users...");
var usersResult = await userRepo.GetAllAsync();
if (usersResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {usersResult.Value?.Count() ?? 0} users");
    Console.ResetColor();
    
    foreach (var user in usersResult.Value?.Take(3) ?? [])
    {
        Console.WriteLine($"   - {user.DisplayName} | {user.Email}");
        Console.WriteLine($"     📍 {user.Address.FullAddress}");
        Console.WriteLine($"     🏢 {user.Company.Name}");
    }
}

// Buscar por username
Console.WriteLine("\n2. Getting user by username 'Bret'...");
var bretResult = await userRepo.GetByUsernameAsync("Bret");
if (bretResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found: {bretResult.Value?.Name}");
    Console.ResetColor();
}

// Buscar por ciudad
Console.WriteLine("\n3. Getting users from city 'Gwenborough'...");
var cityResult = await userRepo.GetByCityAsync("Gwenborough");
if (cityResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {cityResult.Value?.Count() ?? 0} users");
    Console.ResetColor();
}

// Prueba de PostRepository
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📝 POST REPOSITORY TEST");
Console.WriteLine("═══════════════════════════════════════════════════");

var postRepo = serviceProvider.GetRequiredService<IPostRepository>();

// Obtener posts de un usuario
Console.WriteLine("\n1. Getting posts for user #1...");
var userPostsResult = await postRepo.GetByUserIdAsync(1);
if (userPostsResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {userPostsResult.Value?.Count() ?? 0} posts");
    Console.ResetColor();
    
    foreach (var post in userPostsResult.Value?.Take(2) ?? [])
    {
        Console.WriteLine($"   - [{post.Id}] {post.ShortTitle}");
        Console.WriteLine($"     Words: {post.WordCount}");
    }
}

// Buscar posts por título
Console.WriteLine("\n2. Searching posts with 'qui'...");
var searchResult = await postRepo.SearchByTitleAsync("qui");
if (searchResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {searchResult.Value?.Count() ?? 0} matching posts");
    Console.ResetColor();
}

// Obtener comentarios de un post
Console.WriteLine("\n3. Getting comments for post #1...");
var commentsResult = await postRepo.GetCommentsForPostAsync(1);
if (commentsResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {commentsResult.Value?.Count() ?? 0} comments");
    Console.ResetColor();
    
    foreach (var comment in commentsResult.Value?.Take(2) ?? [])
    {
        Console.WriteLine($"   - {comment.ShortName}");
        Console.WriteLine($"     By: {comment.Email}");
    }
}

// Prueba de TodoRepository
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("✅ TODO REPOSITORY TEST");
Console.WriteLine("═══════════════════════════════════════════════════");

var todoRepo = serviceProvider.GetRequiredService<ITodoRepository>();

// Estadísticas de todos
Console.WriteLine("\n1. Getting todo statistics...");
var allTodos = await todoRepo.GetAllAsync();
var completedTodos = await todoRepo.GetCompletedAsync();
var pendingTodos = await todoRepo.GetPendingAsync();

if (allTodos.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Total: {allTodos.Value?.Count() ?? 0}");
    Console.WriteLine($"   ✓ Completed: {completedTodos.Value?.Count() ?? 0}");
    Console.WriteLine($"   ✓ Pending: {pendingTodos.Value?.Count() ?? 0}");
    Console.ResetColor();
}

// Simular toggle de un todo
Console.WriteLine("\n2. Toggling todo #1 completion status...");
var todoResult = await todoRepo.GetByIdAsync(1);
if (todoResult.IsSuccess)
{
    Console.WriteLine($"   Before: {todoResult.Value?.Status}");
    var toggleResult = await todoRepo.ToggleCompletedAsync(1);
    if (toggleResult.IsSuccess)
    {
        Console.WriteLine($"   After: {toggleResult.Value?.Status}");
    }
}

// Prueba de AlbumRepository
Console.WriteLine("\n═══════════════════════════════════════════════════");
Console.WriteLine("📸 ALBUM REPOSITORY TEST");
Console.WriteLine("═══════════════════════════════════════════════════");

var albumRepo = serviceProvider.GetRequiredService<IAlbumRepository>();

// Obtener álbumes de un usuario
Console.WriteLine("\n1. Getting albums for user #1...");
var albumsResult = await albumRepo.GetByUserIdAsync(1);
if (albumsResult.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Found {albumsResult.Value?.Count() ?? 0} albums");
    Console.ResetColor();
}

// Obtener álbum con fotos
Console.WriteLine("\n2. Getting album #1 with photos...");
var albumWithPhotosResult = await albumRepo.GetWithPhotosAsync(1);
if (albumWithPhotosResult.IsSuccess)
{
    var album = albumWithPhotosResult.Value!;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"   ✓ Album: {album.Title}");
    Console.WriteLine($"   ✓ Photos: {album.PhotoCount}");
    Console.ResetColor();
    
    foreach (var photo in album.Photos.Take(3))
    {
        Console.WriteLine($"   - [{photo.Id}] {photo.Title[..Math.Min(40, photo.Title.Length)]}...");
    }
}

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("✓ All repository tests completed!");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();