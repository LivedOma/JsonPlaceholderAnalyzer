using JsonPlaceholderAnalyzer.Application.Services;
using JsonPlaceholderAnalyzer.Console.Handlers;

namespace JsonPlaceholderAnalyzer.Console.UI;

/// <summary>
/// Aplicación principal de consola.
/// Demuestra: Pattern Matching para menús, async/await en UI.
/// </summary>
public class ConsoleApp
{
    private readonly UserService _userService;
    private readonly PostService _postService;
    private readonly TodoService _todoService;
    private readonly AlbumService _albumService;
    private readonly AnalyticsService _analyticsService;
    private readonly QueryService _queryService;
    private readonly NotificationService _notificationService;
    private readonly ConsoleEventHandlers _eventHandlers;
    
    private bool _dataLoaded = false;

    public ConsoleApp(
        UserService userService,
        PostService postService,
        TodoService todoService,
        AlbumService albumService,
        AnalyticsService analyticsService,
        QueryService queryService,
        NotificationService notificationService)
    {
        _userService = userService;
        _postService = postService;
        _todoService = todoService;
        _albumService = albumService;
        _analyticsService = analyticsService;
        _queryService = queryService;
        _notificationService = notificationService;
        _eventHandlers = new ConsoleEventHandlers(notificationService);
    }

    /// <summary>
    /// Punto de entrada principal de la aplicación.
    /// </summary>
    public async Task RunAsync()
    {
        ConsoleHelper.Clear();
        ShowWelcome();
        
        await LoadInitialDataAsync();
        
        var running = true;
        while (running)
        {
            running = await ShowMainMenuAsync();
        }
        
        ShowGoodbye();
    }

    private void ShowWelcome()
    {
        ConsoleHelper.WriteHeader("JSONPlaceholder Analyzer");
        System.Console.WriteLine();
        ConsoleHelper.WriteInfo("Aplicación de demostración de características de C#");
        ConsoleHelper.WriteInfo("Consumiendo datos de JSONPlaceholder API");
        System.Console.WriteLine();
    }

    private void ShowGoodbye()
    {
        System.Console.WriteLine();
        ConsoleHelper.WriteHeader("¡Hasta pronto!");
        ConsoleHelper.WriteInfo("Gracias por usar JSONPlaceholder Analyzer");
    }

    private async Task LoadInitialDataAsync()
    {
        ConsoleHelper.WriteInfo("Cargando datos iniciales...");
        
        var result = await _analyticsService.LoadDataAsync();
        
        if (result.IsSuccess)
        {
            _dataLoaded = true;
            ConsoleHelper.WriteSuccess("Datos cargados correctamente");
        }
        else
        {
            ConsoleHelper.WriteError($"Error al cargar datos: {result.Error}");
        }
        
        ConsoleHelper.Pause();
    }

    /// <summary>
    /// Menú principal usando Pattern Matching.
    /// </summary>
    private async Task<bool> ShowMainMenuAsync()
    {
        ConsoleHelper.Clear();
        ConsoleHelper.WriteHeader("Menú Principal");
        
        var option = ConsoleHelper.ShowMenu("Seleccione una opción",
            "👤 Gestión de Usuarios",
            "📝 Gestión de Posts",
            "✅ Gestión de Tareas (Todos)",
            "📸 Gestión de Álbumes",
            "📊 Análisis y Estadísticas",
            "🔍 Búsquedas Avanzadas",
            "⚙️ Configuración",
            "🚪 Salir"
        );

        // Pattern Matching con switch expression
        return option switch
        {
            0 => await HandleUsersMenuAsync(),
            1 => await HandlePostsMenuAsync(),
            2 => await HandleTodosMenuAsync(),
            3 => await HandleAlbumsMenuAsync(),
            4 => await HandleAnalyticsMenuAsync(),
            5 => await HandleSearchMenuAsync(),
            6 => await HandleConfigMenuAsync(),
            7 => false, // Salir
            _ => true   // Opción no válida, continuar
        };
    }

    #region Users Menu

    private async Task<bool> HandleUsersMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Gestión de Usuarios");
            
            var option = ConsoleHelper.ShowMenu("Usuarios",
                "Listar todos los usuarios",
                "Buscar usuario por ID",
                "Buscar usuario por username",
                "Buscar usuarios por ciudad",
                "Ver resumen de usuarios",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => await ListAllUsersAsync(),
                1 => await SearchUserByIdAsync(),
                2 => await SearchUserByUsernameAsync(),
                3 => await SearchUsersByCityAsync(),
                4 => await ShowUserSummaryAsync(),
                5 => false,
                _ => true
            };
        }
        return true;
    }

    private async Task<bool> ListAllUsersAsync()
    {
        ConsoleHelper.WriteSubHeader("Todos los usuarios");
        
        var result = await _userService.GetAllAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
            ConsoleHelper.Pause();
            return true;
        }

        var users = result.Value!.ToList();
        
        ConsoleHelper.WriteTable(users,
            ("ID", u => u.Id.ToString(), 4),
            ("Nombre", u => u.Name, 25),
            ("Username", u => u.Username, 15),
            ("Email", u => u.Email, 25),
            ("Ciudad", u => u.Address.City, 15)
        );
        
        ConsoleHelper.WriteInfo($"Total: {users.Count} usuarios");
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> SearchUserByIdAsync()
    {
        ConsoleHelper.WriteSubHeader("Buscar usuario por ID");
        
        var id = ConsoleHelper.ReadInt("Ingrese el ID del usuario");
        
        if (id is null)
        {
            ConsoleHelper.WriteError("ID inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _userService.GetByIdAsync(id.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var user = result.Value!;
            System.Console.WriteLine();
            ConsoleHelper.WriteItem($"ID: {user.Id}");
            ConsoleHelper.WriteItem($"Nombre: {user.Name}");
            ConsoleHelper.WriteItem($"Username: @{user.Username}");
            ConsoleHelper.WriteItem($"Email: {user.Email}");
            ConsoleHelper.WriteItem($"Teléfono: {user.Phone ?? "N/A"}");
            ConsoleHelper.WriteItem($"Website: {user.Website ?? "N/A"}");
            ConsoleHelper.WriteItem($"Dirección: {user.Address.FullAddress}");
            ConsoleHelper.WriteItem($"Compañía: {user.Company.Name}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> SearchUserByUsernameAsync()
    {
        ConsoleHelper.WriteSubHeader("Buscar usuario por username");
        
        var username = ConsoleHelper.ReadLine("Ingrese el username");
        
        if (string.IsNullOrWhiteSpace(username))
        {
            ConsoleHelper.WriteError("Username inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _userService.GetByUsernameAsync(username);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var user = result.Value!;
            ConsoleHelper.WriteSuccess($"Usuario encontrado: {user.Name} ({user.Email})");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> SearchUsersByCityAsync()
    {
        ConsoleHelper.WriteSubHeader("Buscar usuarios por ciudad");
        
        var city = ConsoleHelper.ReadLine("Ingrese la ciudad");
        
        if (string.IsNullOrWhiteSpace(city))
        {
            ConsoleHelper.WriteError("Ciudad inválida");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _userService.GetByCityAsync(city);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var users = result.Value!.ToList();
            
            if (users.Count == 0)
            {
                ConsoleHelper.WriteWarning($"No se encontraron usuarios en {city}");
            }
            else
            {
                ConsoleHelper.WriteSuccess($"Se encontraron {users.Count} usuarios:");
                foreach (var user in users)
                {
                    ConsoleHelper.WriteItem($"{user.Name} ({user.Email})");
                }
            }
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowUserSummaryAsync()
    {
        ConsoleHelper.WriteSubHeader("Resumen de usuarios");
        
        var result = await _userService.GetUserSummaryAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var summary = result.Value!;
            ConsoleHelper.WriteItem($"Total de usuarios: {summary.TotalUsers}");
            ConsoleHelper.WriteItem($"Compañías únicas: {summary.UniqueCompanies}");
            ConsoleHelper.WriteItem($"Ciudades únicas: {summary.UniqueCities}");
            ConsoleHelper.WriteItem($"Usuarios con website: {summary.UsersWithWebsite}");
            ConsoleHelper.WriteItem($"Usuarios con teléfono: {summary.UsersWithPhone}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    #endregion

    #region Posts Menu

    private async Task<bool> HandlePostsMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Gestión de Posts");
            
            var option = ConsoleHelper.ShowMenu("Posts",
                "Listar posts (paginado)",
                "Buscar post por ID",
                "Buscar posts por título",
                "Ver posts de un usuario",
                "Ver estadísticas de posts",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => await ListPostsPaginatedAsync(),
                1 => await SearchPostByIdAsync(),
                2 => await SearchPostsByTitleAsync(),
                3 => await ListPostsByUserAsync(),
                4 => await ShowPostStatisticsAsync(),
                5 => false,
                _ => true
            };
        }
        return true;
    }

    private async Task<bool> ListPostsPaginatedAsync()
    {
        var page = 1;
        var pageSize = 10;
        var browsing = true;

        while (browsing)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteSubHeader($"Posts - Página {page}");
            
            var result = await _postService.GetAllAsync();
            
            if (result.IsFailure)
            {
                ConsoleHelper.WriteError(result.Error!);
                ConsoleHelper.Pause();
                return true;
            }

            var allPosts = result.Value!.ToList();
            var totalPages = (int)Math.Ceiling((double)allPosts.Count / pageSize);
            var posts = allPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ConsoleHelper.WriteTable(posts,
                ("ID", p => p.Id.ToString(), 4),
                ("Usuario", p => p.UserId.ToString(), 7),
                ("Título", p => p.ShortTitle, 40),
                ("Palabras", p => p.WordCount.ToString(), 8)
            );

            ConsoleHelper.WriteInfo($"Página {page} de {totalPages} ({allPosts.Count} posts totales)");
            System.Console.WriteLine();
            
            System.Console.WriteLine("    [A] Anterior  [S] Siguiente  [V] Volver");
            var key = System.Console.ReadKey(true).KeyChar;

            var lowerKey = char.ToLower(key);

            if (lowerKey == 'a' && page > 1)
                page--;
            else if (lowerKey == 's' && page < totalPages)
                page++;

            browsing = lowerKey != 'v';
        }
        return true;
    }

    private async Task<bool> SearchPostByIdAsync()
    {
        ConsoleHelper.WriteSubHeader("Buscar post por ID");
        
        var id = ConsoleHelper.ReadInt("Ingrese el ID del post");
        
        if (id is null)
        {
            ConsoleHelper.WriteError("ID inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _postService.GetWithCommentsAsync(id.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var data = result.Value!;
            System.Console.WriteLine();
            ConsoleHelper.WriteItem($"ID: {data.Post.Id}");
            ConsoleHelper.WriteItem($"Título: {data.Post.Title}");
            ConsoleHelper.WriteItem($"Cuerpo: {data.Post.Body[..Math.Min(200, data.Post.Body.Length)]}...");
            ConsoleHelper.WriteItem($"Palabras: {data.Post.WordCount}");
            ConsoleHelper.WriteItem($"Comentarios: {data.CommentCount}");
            
            if (data.CommentCount > 0)
            {
                System.Console.WriteLine();
                ConsoleHelper.WriteInfo("Primeros 3 comentarios:");
                foreach (var comment in data.Comments.Take(3))
                {
                    ConsoleHelper.WriteItem($"{comment.Email}: {comment.ShortName}", ConsoleColor.DarkGray);
                }
            }
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> SearchPostsByTitleAsync()
    {
        ConsoleHelper.WriteSubHeader("Buscar posts por título");
        
        var searchTerm = ConsoleHelper.ReadLine("Ingrese el término de búsqueda");
        
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            ConsoleHelper.WriteError("Término de búsqueda inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _postService.SearchByTitleAsync(searchTerm);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var posts = result.Value!.ToList();
            
            if (posts.Count == 0)
            {
                ConsoleHelper.WriteWarning($"No se encontraron posts con '{searchTerm}'");
            }
            else
            {
                ConsoleHelper.WriteSuccess($"Se encontraron {posts.Count} posts:");
                ConsoleHelper.WriteTable(posts.Take(10),
                    ("ID", p => p.Id.ToString(), 4),
                    ("Título", p => p.ShortTitle, 50)
                );
            }
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ListPostsByUserAsync()
    {
        ConsoleHelper.WriteSubHeader("Posts de un usuario");
        
        var userId = ConsoleHelper.ReadInt("Ingrese el ID del usuario");
        
        if (userId is null)
        {
            ConsoleHelper.WriteError("ID de usuario inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _postService.GetByUserIdAsync(userId.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var posts = result.Value!.ToList();
            ConsoleHelper.WriteSuccess($"Posts del usuario #{userId}: {posts.Count}");
            
            ConsoleHelper.WriteTable(posts,
                ("ID", p => p.Id.ToString(), 4),
                ("Título", p => p.ShortTitle, 45),
                ("Palabras", p => p.WordCount.ToString(), 8)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowPostStatisticsAsync()
    {
        ConsoleHelper.WriteSubHeader("Estadísticas de posts");
        
        var result = await _postService.GetStatisticsAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var stats = result.Value!;
            ConsoleHelper.WriteItem($"Total de posts: {stats.TotalPosts}");
            ConsoleHelper.WriteItem($"Total de palabras: {stats.TotalWords:N0}");
            ConsoleHelper.WriteItem($"Promedio de palabras/post: {stats.AverageWordsPerPost:F1}");
            ConsoleHelper.WriteItem($"Post más largo: #{stats.LongestPost?.Id} ({stats.LongestPost?.WordCount} palabras)");
            ConsoleHelper.WriteItem($"Post más corto: #{stats.ShortestPost?.Id} ({stats.ShortestPost?.WordCount} palabras)");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    #endregion

    #region Todos Menu

    private async Task<bool> HandleTodosMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Gestión de Tareas");
            
            var option = ConsoleHelper.ShowMenu("Tareas",
                "Ver todos pendientes",
                "Ver todos completados",
                "Ver todos por usuario",
                "Cambiar estado de una tarea",
                "Ver estadísticas de tareas",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => await ListPendingTodosAsync(),
                1 => await ListCompletedTodosAsync(),
                2 => await ListTodosByUserAsync(),
                3 => await ToggleTodoStatusAsync(),
                4 => await ShowTodoStatisticsAsync(),
                5 => false,
                _ => true
            };
        }
        return true;
    }

    private async Task<bool> ListPendingTodosAsync()
    {
        ConsoleHelper.WriteSubHeader("Tareas pendientes");
        
        var result = await _todoService.GetPendingAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var todos = result.Value!.ToList();
            ConsoleHelper.WriteSuccess($"Tareas pendientes: {todos.Count}");
            
            ConsoleHelper.WriteTable(todos.Take(20),
                ("ID", t => t.Id.ToString(), 4),
                ("Usuario", t => t.UserId.ToString(), 7),
                ("Título", t => t.Title.Length > 40 ? t.Title[..37] + "..." : t.Title, 45),
                ("Prioridad", t => t.Priority.ToString(), 10)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ListCompletedTodosAsync()
    {
        ConsoleHelper.WriteSubHeader("Tareas completadas");
        
        var result = await _todoService.GetCompletedAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var todos = result.Value!.ToList();
            ConsoleHelper.WriteSuccess($"Tareas completadas: {todos.Count}");
            
            ConsoleHelper.WriteTable(todos.Take(20),
                ("ID", t => t.Id.ToString(), 4),
                ("Usuario", t => t.UserId.ToString(), 7),
                ("Título", t => t.Title.Length > 50 ? t.Title[..47] + "..." : t.Title, 55)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ListTodosByUserAsync()
    {
        ConsoleHelper.WriteSubHeader("Tareas por usuario");
        
        var userId = ConsoleHelper.ReadInt("Ingrese el ID del usuario");
        
        if (userId is null)
        {
            ConsoleHelper.WriteError("ID de usuario inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _todoService.GetByUserIdAsync(userId.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var todos = result.Value!.ToList();
            var completed = todos.Count(t => t.Completed);
            var pending = todos.Count - completed;
            
            ConsoleHelper.WriteSuccess($"Tareas del usuario #{userId}: {todos.Count} (✓{completed} / ○{pending})");
            
            ConsoleHelper.WriteTable(todos,
                ("ID", t => t.Id.ToString(), 4),
                ("Estado", t => t.Completed ? "✓" : "○", 6),
                ("Título", t => t.Title.Length > 50 ? t.Title[..47] + "..." : t.Title, 55)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ToggleTodoStatusAsync()
    {
        ConsoleHelper.WriteSubHeader("Cambiar estado de tarea");
        
        var id = ConsoleHelper.ReadInt("Ingrese el ID de la tarea");
        
        if (id is null)
        {
            ConsoleHelper.WriteError("ID inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _todoService.ToggleCompletedAsync(id.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var todo = result.Value!;
            ConsoleHelper.WriteSuccess($"Tarea #{todo.Id} ahora está: {todo.Status}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowTodoStatisticsAsync()
    {
        ConsoleHelper.WriteSubHeader("Estadísticas de tareas");
        
        var result = await _todoService.GetStatisticsAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var stats = result.Value!;
            ConsoleHelper.WriteItem($"Total de tareas: {stats.Total}");
            ConsoleHelper.WriteItem($"Completadas: {stats.Completed}");
            ConsoleHelper.WriteItem($"Pendientes: {stats.Pending}");
            ConsoleHelper.WriteItem($"Tasa de completado: {stats.CompletionRate:F1}%");
            ConsoleHelper.WriteItem($"Alta prioridad: {stats.HighPriority}");
            ConsoleHelper.WriteItem($"Media prioridad: {stats.MediumPriority}");
            ConsoleHelper.WriteItem($"Baja prioridad: {stats.LowPriority}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    #endregion

    #region Albums Menu

    private async Task<bool> HandleAlbumsMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Gestión de Álbumes");
            
            var option = ConsoleHelper.ShowMenu("Álbumes",
                "Listar todos los álbumes",
                "Ver álbumes de un usuario",
                "Ver álbum con fotos",
                "Ver estadísticas de álbumes",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => await ListAllAlbumsAsync(),
                1 => await ListAlbumsByUserAsync(),
                2 => await ShowAlbumWithPhotosAsync(),
                3 => await ShowAlbumStatisticsAsync(),
                4 => false,
                _ => true
            };
        }
        return true;
    }

    private async Task<bool> ListAllAlbumsAsync()
    {
        ConsoleHelper.WriteSubHeader("Todos los álbumes");
        
        var result = await _albumService.GetAllAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var albums = result.Value!.ToList();
            
            ConsoleHelper.WriteTable(albums.Take(20),
                ("ID", a => a.Id.ToString(), 4),
                ("Usuario", a => a.UserId.ToString(), 7),
                ("Título", a => a.Title.Length > 55 ? a.Title[..52] + "..." : a.Title, 60)
            );
            
            ConsoleHelper.WriteInfo($"Mostrando 20 de {albums.Count} álbumes");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ListAlbumsByUserAsync()
    {
        ConsoleHelper.WriteSubHeader("Álbumes por usuario");
        
        var userId = ConsoleHelper.ReadInt("Ingrese el ID del usuario");
        
        if (userId is null)
        {
            ConsoleHelper.WriteError("ID de usuario inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _albumService.GetByUserIdAsync(userId.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var albums = result.Value!.ToList();
            ConsoleHelper.WriteSuccess($"Álbumes del usuario #{userId}: {albums.Count}");
            
            ConsoleHelper.WriteTable(albums,
                ("ID", a => a.Id.ToString(), 4),
                ("Título", a => a.Title.Length > 60 ? a.Title[..57] + "..." : a.Title, 65)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowAlbumWithPhotosAsync()
    {
        ConsoleHelper.WriteSubHeader("Álbum con fotos");
        
        var albumId = ConsoleHelper.ReadInt("Ingrese el ID del álbum");
        
        if (albumId is null)
        {
            ConsoleHelper.WriteError("ID de álbum inválido");
            ConsoleHelper.Pause();
            return true;
        }

        var result = await _albumService.GetWithPhotosAsync(albumId.Value);
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var album = result.Value!;
            ConsoleHelper.WriteItem($"ID: {album.Id}");
            ConsoleHelper.WriteItem($"Título: {album.Title}");
            ConsoleHelper.WriteItem($"Usuario: {album.UserId}");
            ConsoleHelper.WriteItem($"Fotos: {album.PhotoCount}");
            
            System.Console.WriteLine();
            ConsoleHelper.WriteInfo("Primeras 10 fotos:");
            
            ConsoleHelper.WriteTable(album.Photos.Take(10),
                ("ID", p => p.Id.ToString(), 4),
                ("Título", p => p.Title.Length > 50 ? p.Title[..47] + "..." : p.Title, 55)
            );
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowAlbumStatisticsAsync()
    {
        ConsoleHelper.WriteSubHeader("Estadísticas de álbumes");
        
        var result = await _albumService.GetStatisticsAsync();
        
        if (result.IsFailure)
        {
            ConsoleHelper.WriteError(result.Error!);
        }
        else
        {
            var stats = result.Value!;
            ConsoleHelper.WriteItem($"Total de álbumes: {stats.TotalAlbums}");
            ConsoleHelper.WriteItem($"Fotos estimadas: {stats.EstimatedTotalPhotos:N0}");
            ConsoleHelper.WriteItem($"Promedio álbumes/usuario: {stats.AverageAlbumsPerUser:F1}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    #endregion

    #region Analytics Menu

    private async Task<bool> HandleAnalyticsMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Análisis y Estadísticas");
            
            var option = ConsoleHelper.ShowMenu("Análisis",
                "Análisis completo",
                "Usuarios más activos",
                "Palabras más frecuentes",
                "Distribución de tareas",
                "Estadísticas de contenido",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => await ShowComprehensiveAnalysisAsync(),
                1 => await ShowMostActiveUsersAsync(),
                2 => await ShowTopWordsAsync(),
                3 => await ShowTodoDistributionAsync(),
                4 => await ShowContentStatsAsync(),
                5 => false,
                _ => true
            };
        }
        return true;
    }

    private async Task<bool> ShowComprehensiveAnalysisAsync()
    {
        ConsoleHelper.WriteSubHeader("Análisis Completo");
        ConsoleHelper.WriteInfo("Procesando datos...");
        
        await Task.Delay(500); // Pequeña pausa para efecto visual
        
        var analysis = _analyticsService.GetComprehensiveAnalysis();
        
        System.Console.WriteLine();
        ConsoleHelper.WriteSubHeader("🏆 Top 5 Usuarios Más Activos");
        ConsoleHelper.WriteTable(analysis.MostActiveUsers,
            ("Usuario", u => u.UserName, 25),
            ("Posts", u => u.PostCount.ToString(), 6),
            ("Todos ✓", u => u.CompletedTodos.ToString(), 8),
            ("Tasa %", u => $"{u.TodoCompletionRate:F0}%", 7)
        );
        
        ConsoleHelper.WriteSubHeader("📝 Top 10 Palabras en Títulos");
        foreach (var word in analysis.TopWords.Take(10))
        {
            var bar = new string('█', Math.Min(word.Count, 20));
            System.Console.WriteLine($"    {word.Word.PadRight(15)} {bar} ({word.Count})");
        }
        
        ConsoleHelper.WriteSubHeader("📊 Resumen General");
        ConsoleHelper.WriteItem($"Posts analizados: {analysis.TotalAnalyzedPosts}");
        ConsoleHelper.WriteItem($"Tareas analizadas: {analysis.TotalAnalyzedTodos}");
        ConsoleHelper.WriteItem($"Tasa global de completado: {analysis.OverallCompletionRate:F1}%");
        
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ShowMostActiveUsersAsync()
    {
        ConsoleHelper.WriteSubHeader("Usuarios Más Activos");
        
        var analysis = _analyticsService.GetComprehensiveAnalysis();
        
        ConsoleHelper.WriteTable(analysis.MostActiveUsers,
            ("ID", u => u.UserId.ToString(), 4),
            ("Nombre", u => u.UserName, 25),
            ("Posts", u => u.PostCount.ToString(), 6),
            ("Todos Completados", u => u.CompletedTodos.ToString(), 17),
            ("Tasa Completado", u => $"{u.TodoCompletionRate:F1}%", 15)
        );
        
        ConsoleHelper.Pause();
        return await Task.FromResult(true);
    }

    private async Task<bool> ShowTopWordsAsync()
    {
        ConsoleHelper.WriteSubHeader("Palabras Más Frecuentes en Títulos");
        
        var analysis = _analyticsService.GetComprehensiveAnalysis();
        
        System.Console.WriteLine();
        foreach (var (word, index) in analysis.TopWords.Select((w, i) => (w, i)))
        {
            var barLength = (int)((double)word.Count / analysis.TopWords.First().Count * 30);
            var bar = new string('█', barLength);
            
            System.Console.ForegroundColor = index switch
            {
                0 => ConsoleColor.Yellow,
                1 => ConsoleColor.White,
                2 => ConsoleColor.Gray,
                _ => ConsoleColor.DarkGray
            };
            
            System.Console.WriteLine($"    {(index + 1).ToString().PadLeft(2)}. {word.Word.PadRight(15)} {bar} {word.Count}");
        }
        System.Console.ResetColor();
        
        ConsoleHelper.Pause();
        return await Task.FromResult(true);
    }

    private async Task<bool> ShowTodoDistributionAsync()
    {
        ConsoleHelper.WriteSubHeader("Distribución de Tareas por Usuario");
        
        var analysis = _analyticsService.GetComprehensiveAnalysis();
        
        ConsoleHelper.WriteTable(analysis.TodoDistribution,
            ("Usuario", d => d.UserName, 25),
            ("✓ Completadas", d => d.Completed.ToString(), 13),
            ("○ Pendientes", d => d.Pending.ToString(), 12),
            ("🔥 Urgentes", d => d.HighPriority.ToString(), 11)
        );
        
        ConsoleHelper.Pause();
        return await Task.FromResult(true);
    }

    private async Task<bool> ShowContentStatsAsync()
    {
        ConsoleHelper.WriteSubHeader("Estadísticas de Contenido");
        
        var wordStats = _analyticsService.GetWordStatistics();
        var counts = _analyticsService.GetBasicCounts();
        
        ConsoleHelper.WriteItem($"Total de usuarios: {counts.TotalUsers}");
        ConsoleHelper.WriteItem($"Total de posts: {counts.TotalPosts}");
        ConsoleHelper.WriteItem($"Total de tareas: {counts.TotalTodos}");
        ConsoleHelper.WriteItem($"  - Completadas: {counts.CompletedTodos}");
        ConsoleHelper.WriteItem($"  - Pendientes: {counts.PendingTodos}");
        
        System.Console.WriteLine();
        ConsoleHelper.WriteItem($"Total de palabras en posts: {wordStats.TotalWords:N0}");
        ConsoleHelper.WriteItem($"Promedio palabras/post: {wordStats.AverageWords:F1}");
        ConsoleHelper.WriteItem($"Post más corto: {wordStats.MinWords} palabras");
        ConsoleHelper.WriteItem($"Post más largo: {wordStats.MaxWords} palabras");
        
        ConsoleHelper.Pause();
        return await Task.FromResult(true);
    }

    #endregion

    #region Search Menu

    private async Task<bool> HandleSearchMenuAsync()
    {
        ConsoleHelper.Clear();
        ConsoleHelper.WriteHeader("Búsquedas Avanzadas");
        ConsoleHelper.WriteInfo("Esta funcionalidad usa QueryService con paginación y filtros");
        ConsoleHelper.Pause();
        return await Task.FromResult(true);
    }

    #endregion

    #region Config Menu

    private Task<bool> HandleConfigMenuAsync()
    {
        var inSubmenu = true;
        while (inSubmenu)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader("Configuración");
            
            var option = ConsoleHelper.ShowMenu("Configuración",
                $"Modo verbose: {(_eventHandlers.VerboseMode ? "ON" : "OFF")}",
                "Recargar datos",
                "Acerca de",
                "Volver al menú principal"
            );

            inSubmenu = option switch
            {
                0 => ToggleVerboseMode(),
                1 => ReloadDataAsync().GetAwaiter().GetResult(),
                2 => ShowAbout(),
                3 => false,
                _ => true
            };
        }
        return Task.FromResult(true);
    }

    private bool ToggleVerboseMode()
    {
        _eventHandlers.VerboseMode = !_eventHandlers.VerboseMode;
        ConsoleHelper.WriteSuccess($"Modo verbose: {(_eventHandlers.VerboseMode ? "ON" : "OFF")}");
        ConsoleHelper.Pause();
        return true;
    }

    private async Task<bool> ReloadDataAsync()
    {
        ConsoleHelper.WriteInfo("Recargando datos...");
        var result = await _analyticsService.LoadDataAsync();
        
        if (result.IsSuccess)
        {
            ConsoleHelper.WriteSuccess("Datos recargados correctamente");
        }
        else
        {
            ConsoleHelper.WriteError($"Error: {result.Error}");
        }
        
        ConsoleHelper.Pause();
        return true;
    }

    private bool ShowAbout()
    {
        ConsoleHelper.Clear();
        ConsoleHelper.WriteHeader("Acerca de");
        System.Console.WriteLine();
        ConsoleHelper.WriteItem("JSONPlaceholder Analyzer v1.0");
        ConsoleHelper.WriteItem("Aplicación de demostración de C# moderno");
        System.Console.WriteLine();
        ConsoleHelper.WriteInfo("Características de C# demostradas:");
        ConsoleHelper.WriteItem("• Records y Init-only properties", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Pattern Matching avanzado", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Async/Await", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• LINQ completo", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Generics con constraints", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Delegates y Events", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Primary Constructors (C# 12)", ConsoleColor.Cyan);
        ConsoleHelper.WriteItem("• Operadores modernos (??, ?., ??=)", ConsoleColor.Cyan);
        ConsoleHelper.Pause();
        return true;
    }

    #endregion
}