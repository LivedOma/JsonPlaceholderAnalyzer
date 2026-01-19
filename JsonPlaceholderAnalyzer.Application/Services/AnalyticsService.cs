using JsonPlaceholderAnalyzer.Domain.Common;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Enums;

namespace JsonPlaceholderAnalyzer.Application.Services;

/// <summary>
/// Servicio de análisis que demuestra TODAS las operaciones de LINQ.
/// Este es el servicio más importante para practicar LINQ.
/// </summary>
public class AnalyticsService(
    UserService userService,
    PostService postService,
    TodoService todoService,
    AlbumService albumService,
    NotificationService notificationService)
{
    #region Datos cacheados para análisis
    
    private List<User>? _users;
    private List<Post>? _posts;
    private List<Todo>? _todos;
    private List<Album>? _albums;
    
    /// <summary>
    /// Carga todos los datos necesarios para el análisis.
    /// </summary>
    public async Task<Result> LoadDataAsync(CancellationToken cancellationToken = default)
    {
        notificationService.OnNotification("Cargando datos para análisis...");
        
        var usersResult = await userService.GetAllAsync(cancellationToken);
        var postsResult = await postService.GetAllAsync(cancellationToken);
        var todosResult = await todoService.GetAllAsync(cancellationToken);
        var albumsResult = await albumService.GetAllAsync(cancellationToken);
        
        if (usersResult.IsFailure) return Result.Failure(usersResult.Error!);
        if (postsResult.IsFailure) return Result.Failure(postsResult.Error!);
        if (todosResult.IsFailure) return Result.Failure(todosResult.Error!);
        if (albumsResult.IsFailure) return Result.Failure(albumsResult.Error!);
        
        _users = usersResult.Value!.ToList();
        _posts = postsResult.Value!.ToList();
        _todos = todosResult.Value!.ToList();
        _albums = albumsResult.Value!.ToList();
        
        notificationService.OnNotification(
            $"Datos cargados: {_users.Count} usuarios, {_posts.Count} posts, " +
            $"{_todos.Count} todos, {_albums.Count} álbumes"
        );
        
        return Result.Success();
    }
    
    private void EnsureDataLoaded()
    {
        if (_users is null || _posts is null || _todos is null || _albums is null)
            throw new InvalidOperationException("Debe llamar a LoadDataAsync primero");
    }
    
    #endregion

    #region 1. OPERADORES DE PROYECCIÓN (Select, SelectMany)

    /// <summary>
    /// SELECT: Transforma cada elemento de una secuencia.
    /// </summary>
    public IEnumerable<string> GetUserEmails()
    {
        EnsureDataLoaded();
        
        // Select simple: extrae una propiedad
        return _users!.Select(u => u.Email);
    }

    /// <summary>
    /// SELECT con transformación compleja.
    /// </summary>
    public IEnumerable<UserSummaryDto> GetUserSummaries()
    {
        EnsureDataLoaded();
        
        // Select con proyección a tipo anónimo o DTO
        return _users!.Select(u => new UserSummaryDto
        {
            Id = u.Id,
            FullName = u.Name,
            Email = u.Email,
            City = u.Address.City,
            Company = u.Company.Name,
            PostCount = _posts!.Count(p => p.UserId == u.Id),
            TodoCount = _todos!.Count(t => t.UserId == u.Id),
            CompletedTodoCount = _todos!.Count(t => t.UserId == u.Id && t.Completed)
        });
    }

    /// <summary>
    /// SELECT con índice.
    /// </summary>
    public IEnumerable<string> GetIndexedUserNames()
    {
        EnsureDataLoaded();
        
        // Select con índice: (elemento, índice)
        return _users!.Select((user, index) => $"{index + 1}. {user.Name}");
    }

    /// <summary>
    /// SELECTMANY: Aplana secuencias anidadas.
    /// </summary>
    public IEnumerable<string> GetAllPostTitlesFlattened()
    {
        EnsureDataLoaded();
        
        // SelectMany: para cada usuario, obtiene sus posts y aplana todo en una lista
        return _users!.SelectMany(
            user => _posts!.Where(p => p.UserId == user.Id),
            (user, post) => $"{user.Username}: {post.Title}"
        );
    }

    /// <summary>
    /// SELECTMANY simple: obtener todas las palabras de todos los títulos.
    /// </summary>
    public IEnumerable<string> GetAllWordsFromTitles()
    {
        EnsureDataLoaded();
        
        // Aplana: cada título -> array de palabras -> todas las palabras
        return _posts!
            .SelectMany(p => p.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(w => w.ToLowerInvariant());
    }

    #endregion

    #region 2. OPERADORES DE FILTRADO (Where, OfType, Distinct)

    /// <summary>
    /// WHERE: Filtra elementos que cumplen una condición.
    /// </summary>
    public IEnumerable<User> GetUsersFromSpecificCities(params string[] cities)
    {
        EnsureDataLoaded();
        
        // Where con condición
        return _users!.Where(u => cities.Contains(u.Address.City, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// WHERE con índice.
    /// </summary>
    public IEnumerable<Post> GetEvenIndexedPosts()
    {
        EnsureDataLoaded();
        
        // Where con índice: solo posts en posiciones pares
        return _posts!.Where((post, index) => index % 2 == 0);
    }

    /// <summary>
    /// WHERE múltiple (encadenado).
    /// </summary>
    public IEnumerable<Todo> GetUrgentPendingTodos()
    {
        EnsureDataLoaded();
        
        return _todos!
            .Where(t => !t.Completed)
            .Where(t => t.Priority == TodoPriority.High)
            .Where(t => t.Title.Length > 10);
    }

    /// <summary>
    /// OFTYPE: Filtra por tipo (útil con colecciones de object).
    /// </summary>
    public IEnumerable<string> FilterStringsFromMixed()
    {
        // Ejemplo con colección mixta
        var mixedCollection = new List<object> 
        { 
            "texto1", 123, "texto2", 45.6, "texto3", true 
        };
        
        // OfType filtra solo los strings
        return mixedCollection.OfType<string>();
    }

    /// <summary>
    /// DISTINCT: Elimina duplicados.
    /// </summary>
    public IEnumerable<string> GetUniqueCities()
    {
        EnsureDataLoaded();
        
        return _users!
            .Select(u => u.Address.City)
            .Distinct();
    }

    /// <summary>
    /// DISTINCTBY (C# 9+): Elimina duplicados por una clave.
    /// </summary>
    public IEnumerable<User> GetOneUserPerCompany()
    {
        EnsureDataLoaded();
        
        // DistinctBy: un usuario por cada compañía
        return _users!.DistinctBy(u => u.Company.Name);
    }

    #endregion

    #region 3. OPERADORES DE ORDENAMIENTO (OrderBy, ThenBy)

    /// <summary>
    /// ORDERBY: Ordena ascendente.
    /// </summary>
    public IEnumerable<User> GetUsersSortedByName()
    {
        EnsureDataLoaded();
        
        return _users!.OrderBy(u => u.Name);
    }

    /// <summary>
    /// ORDERBYDESCENDING: Ordena descendente.
    /// </summary>
    public IEnumerable<Post> GetPostsByWordCountDesc()
    {
        EnsureDataLoaded();
        
        return _posts!.OrderByDescending(p => p.WordCount);
    }

    /// <summary>
    /// THENBY: Ordenamiento secundario.
    /// </summary>
    public IEnumerable<User> GetUsersSortedByCityThenName()
    {
        EnsureDataLoaded();
        
        return _users!
            .OrderBy(u => u.Address.City)      // Primario: ciudad
            .ThenBy(u => u.Company.Name)        // Secundario: compañía
            .ThenByDescending(u => u.Name);     // Terciario: nombre (desc)
    }

    /// <summary>
    /// ORDER con comparador personalizado.
    /// </summary>
    public IEnumerable<Post> GetPostsOrderedByTitleLength()
    {
        EnsureDataLoaded();
        
        return _posts!.OrderBy(p => p.Title.Length);
    }

    #endregion

    #region 4. OPERADORES DE AGRUPACIÓN (GroupBy, ToLookup)

    /// <summary>
    /// GROUPBY: Agrupa elementos por una clave.
    /// </summary>
    public IEnumerable<IGrouping<int, Post>> GetPostsGroupedByUser()
    {
        EnsureDataLoaded();
        
        return _posts!.GroupBy(p => p.UserId);
    }

    /// <summary>
    /// GROUPBY con proyección.
    /// </summary>
    public IEnumerable<UserPostStats> GetPostStatsByUser()
    {
        EnsureDataLoaded();
        
        return _posts!
            .GroupBy(p => p.UserId)
            .Select(g => new UserPostStats
            {
                UserId = g.Key,
                UserName = _users!.FirstOrDefault(u => u.Id == g.Key)?.Name ?? "Unknown",
                PostCount = g.Count(),
                TotalWords = g.Sum(p => p.WordCount),
                AverageWords = g.Average(p => p.WordCount),
                LongestPost = g.OrderByDescending(p => p.WordCount).First()
            });
    }

    /// <summary>
    /// GROUPBY con múltiples claves.
    /// </summary>
    public IEnumerable<TodoGroupStats> GetTodosGroupedByUserAndStatus()
    {
        EnsureDataLoaded();
        
        return _todos!
            .GroupBy(t => new { t.UserId, t.Completed })
            .Select(g => new TodoGroupStats
            {
                UserId = g.Key.UserId,
                IsCompleted = g.Key.Completed,
                Count = g.Count(),
                Titles = g.Select(t => t.Title).ToList()
            });
    }

    /// <summary>
    /// TOLOOKUP: Similar a GroupBy pero retorna un Lookup (diccionario de grupos).
    /// </summary>
    public ILookup<string, User> GetUsersLookupByCity()
    {
        EnsureDataLoaded();
        
        // ToLookup crea una estructura de búsqueda rápida
        return _users!.ToLookup(u => u.Address.City);
    }

    #endregion

    #region 5. OPERADORES DE AGREGACIÓN (Count, Sum, Average, Min, Max, Aggregate)

    /// <summary>
    /// COUNT: Cuenta elementos.
    /// </summary>
    public AggregationStats GetBasicCounts()
    {
        EnsureDataLoaded();
        
        return new AggregationStats
        {
            TotalUsers = _users!.Count(),
            TotalPosts = _posts!.Count(),
            TotalTodos = _todos!.Count(),
            CompletedTodos = _todos!.Count(t => t.Completed),
            PendingTodos = _todos!.Count(t => !t.Completed),
            PostsWithLongTitles = _posts!.Count(p => p.Title.Length > 50)
        };
    }

    /// <summary>
    /// SUM: Suma valores.
    /// </summary>
    public int GetTotalWordCount()
    {
        EnsureDataLoaded();
        
        return _posts!.Sum(p => p.WordCount);
    }

    /// <summary>
    /// AVERAGE: Calcula promedio.
    /// </summary>
    public double GetAverageWordsPerPost()
    {
        EnsureDataLoaded();
        
        return _posts!.Average(p => p.WordCount);
    }

    /// <summary>
    /// MIN y MAX: Valores extremos.
    /// </summary>
    public (int Min, int Max, Post? Shortest, Post? Longest) GetWordCountExtremes()
    {
        EnsureDataLoaded();
        
        return (
            Min: _posts!.Min(p => p.WordCount),
            Max: _posts!.Max(p => p.WordCount),
            Shortest: _posts!.MinBy(p => p.WordCount),
            Longest: _posts!.MaxBy(p => p.WordCount)
        );
    }

    /// <summary>
    /// AGGREGATE: Acumulador personalizado.
    /// </summary>
    public string ConcatenateAllUsernames()
    {
        EnsureDataLoaded();
        
        // Aggregate con acumulador
        return _users!.Aggregate(
            seed: "",
            func: (accumulated, user) => 
                string.IsNullOrEmpty(accumulated) 
                    ? user.Username 
                    : $"{accumulated}, {user.Username}"
        );
    }

    /// <summary>
    /// AGGREGATE con selector de resultado.
    /// </summary>
    public WordStats GetWordStatistics()
    {
        EnsureDataLoaded();
        
        return _posts!.Aggregate(
            seed: new WordAccumulator(),
            func: (acc, post) =>
            {
                acc.TotalPosts++;
                acc.TotalWords += post.WordCount;
                if (post.WordCount > acc.MaxWords) acc.MaxWords = post.WordCount;
                if (acc.MinWords == 0 || post.WordCount < acc.MinWords) acc.MinWords = post.WordCount;
                return acc;
            },
            resultSelector: acc => new WordStats
            {
                TotalPosts = acc.TotalPosts,
                TotalWords = acc.TotalWords,
                AverageWords = acc.TotalPosts > 0 ? (double)acc.TotalWords / acc.TotalPosts : 0,
                MinWords = acc.MinWords,
                MaxWords = acc.MaxWords
            }
        );
    }

    #endregion

    #region 6. OPERADORES DE CONJUNTO (Union, Intersect, Except)

    /// <summary>
    /// UNION: Combina dos secuencias eliminando duplicados.
    /// </summary>
    public IEnumerable<int> GetAllUserIdsWithPostsOrTodos()
    {
        EnsureDataLoaded();
        
        var userIdsWithPosts = _posts!.Select(p => p.UserId);
        var userIdsWithTodos = _todos!.Select(t => t.UserId);
        
        // Union combina ambas listas sin duplicados
        return userIdsWithPosts.Union(userIdsWithTodos);
    }

    /// <summary>
    /// INTERSECT: Elementos comunes entre dos secuencias.
    /// </summary>
    public IEnumerable<int> GetUserIdsWithBothPostsAndCompletedTodos()
    {
        EnsureDataLoaded();
        
        var userIdsWithPosts = _posts!.Select(p => p.UserId).Distinct();
        var userIdsWithCompletedTodos = _todos!
            .Where(t => t.Completed)
            .Select(t => t.UserId)
            .Distinct();
        
        // Intersect: solo los que están en ambas listas
        return userIdsWithPosts.Intersect(userIdsWithCompletedTodos);
    }

    /// <summary>
    /// EXCEPT: Elementos en la primera secuencia pero no en la segunda.
    /// </summary>
    public IEnumerable<int> GetUserIdsWithPostsButNoPendingTodos()
    {
        EnsureDataLoaded();
        
        var userIdsWithPosts = _posts!.Select(p => p.UserId).Distinct();
        var userIdsWithPendingTodos = _todos!
            .Where(t => !t.Completed)
            .Select(t => t.UserId)
            .Distinct();
        
        // Except: en posts pero no en todos pendientes
        return userIdsWithPosts.Except(userIdsWithPendingTodos);
    }

    /// <summary>
    /// CONCAT: Une dos secuencias (permite duplicados).
    /// </summary>
    public IEnumerable<string> GetAllTitles()
    {
        EnsureDataLoaded();
        
        var postTitles = _posts!.Select(p => $"[Post] {p.Title}");
        var todoTitles = _todos!.Select(t => $"[Todo] {t.Title}");
        
        // Concat: une sin eliminar duplicados
        return postTitles.Concat(todoTitles);
    }

    #endregion

    #region 7. OPERADORES DE ELEMENTO (First, Last, Single, ElementAt)

    /// <summary>
    /// FIRST, FIRSTORDEFAULT: Primer elemento.
    /// </summary>
    public (User? First, User? FirstFromCity, User? FirstOrDefault) GetFirstUsers()
    {
        EnsureDataLoaded();
        
        return (
            First: _users!.First(),  // Lanza excepción si está vacío
            FirstFromCity: _users!.First(u => u.Address.City == "Gwenborough"),
            FirstOrDefault: _users!.FirstOrDefault(u => u.Address.City == "CiudadInexistente")  // null si no encuentra
        );
    }

    /// <summary>
    /// LAST, LASTORDEFAULT: Último elemento.
    /// </summary>
    public (Post Last, Post? LastLong) GetLastPosts()
    {
        EnsureDataLoaded();
        
        return (
            Last: _posts!.Last(),
            LastLong: _posts!.LastOrDefault(p => p.WordCount > 50)
        );
    }

    /// <summary>
    /// SINGLE, SINGLEORDEFAULT: Exactamente un elemento.
    /// </summary>
    public User? GetSingleUserByUsername(string username)
    {
        EnsureDataLoaded();
        
        // SingleOrDefault: retorna null si no hay coincidencia, excepción si hay más de una
        return _users!.SingleOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// ELEMENTAT, ELEMENTATORDEFAULT: Elemento por índice.
    /// </summary>
    public (Post? Fifth, Post? Hundredth) GetPostsByIndex()
    {
        EnsureDataLoaded();
        
        return (
            Fifth: _posts!.ElementAtOrDefault(4),  // Índice 4 = quinto elemento
            Hundredth: _posts!.ElementAtOrDefault(99)
        );
    }

    /// <summary>
    /// DEFAULTIFEMPTY: Proporciona valor por defecto si la secuencia está vacía.
    /// </summary>
    public IEnumerable<Todo> GetTodosOrDefault(int userId)
    {
        EnsureDataLoaded();
        
        var userTodos = _todos!.Where(t => t.UserId == userId);
        
        // Si no hay todos para el usuario, retorna un todo por defecto
        return userTodos.DefaultIfEmpty(new Todo
        {
            Id = 0,
            UserId = userId,
            Title = "No hay tareas",
            Completed = false
        });
    }

    #endregion

    #region 8. OPERADORES DE CUANTIFICACIÓN (Any, All, Contains)

    /// <summary>
    /// ANY: Verifica si algún elemento cumple la condición.
    /// </summary>
    public QuantificationResults GetQuantificationResults()
    {
        EnsureDataLoaded();
        
        return new QuantificationResults
        {
            // Any sin condición: ¿hay elementos?
            HasUsers = _users!.Any(),
            
            // Any con condición: ¿alguno cumple?
            HasUsersFromGwenborough = _users!.Any(u => u.Address.City == "Gwenborough"),
            HasLongPosts = _posts!.Any(p => p.WordCount > 100),
            HasCompletedTodos = _todos!.Any(t => t.Completed),
            
            // All: ¿todos cumplen?
            AllUsersHaveEmail = _users!.All(u => !string.IsNullOrEmpty(u.Email)),
            AllPostsHaveContent = _posts!.All(p => !string.IsNullOrEmpty(p.Body)),
            AllTodosHaveTitle = _todos!.All(t => !string.IsNullOrEmpty(t.Title)),
            
            // Contains: ¿contiene un elemento específico?
            ContainsUserId1 = _users!.Select(u => u.Id).Contains(1),
            ContainsUserId999 = _users!.Select(u => u.Id).Contains(999)
        };
    }

    #endregion

    #region 9. OPERADORES DE PARTICIÓN (Take, Skip, TakeWhile, SkipWhile)

    /// <summary>
    /// TAKE: Toma los primeros N elementos.
    /// </summary>
    public IEnumerable<Post> GetTopPosts(int count)
    {
        EnsureDataLoaded();
        
        return _posts!
            .OrderByDescending(p => p.WordCount)
            .Take(count);
    }

    /// <summary>
    /// SKIP: Salta los primeros N elementos.
    /// </summary>
    public IEnumerable<User> GetUsersAfterFirst(int skipCount)
    {
        EnsureDataLoaded();
        
        return _users!.Skip(skipCount);
    }

    /// <summary>
    /// TAKE + SKIP: Paginación manual.
    /// </summary>
    public IEnumerable<Post> GetPostsPage(int page, int pageSize)
    {
        EnsureDataLoaded();
        
        return _posts!
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// TAKEWHILE: Toma mientras se cumpla la condición.
    /// </summary>
    public IEnumerable<Post> TakePostsWhileShort()
    {
        EnsureDataLoaded();
        
        // Toma posts mientras tengan menos de 50 palabras (se detiene al encontrar uno largo)
        return _posts!
            .OrderBy(p => p.WordCount)
            .TakeWhile(p => p.WordCount < 50);
    }

    /// <summary>
    /// SKIPWHILE: Salta mientras se cumpla la condición.
    /// </summary>
    public IEnumerable<Post> SkipShortPosts()
    {
        EnsureDataLoaded();
        
        // Salta posts cortos hasta encontrar uno largo
        return _posts!
            .OrderBy(p => p.WordCount)
            .SkipWhile(p => p.WordCount < 30);
    }

    /// <summary>
    /// TAKELAST (C# 9+): Toma los últimos N elementos.
    /// </summary>
    public IEnumerable<Todo> GetLastTodos(int count)
    {
        EnsureDataLoaded();
        
        return _todos!.TakeLast(count);
    }

    /// <summary>
    /// SKIPLAST (C# 9+): Salta los últimos N elementos.
    /// </summary>
    public IEnumerable<Todo> GetTodosExceptLast(int skipCount)
    {
        EnsureDataLoaded();
        
        return _todos!.SkipLast(skipCount);
    }

    /// <summary>
    /// CHUNK (C# 10+): Divide en grupos de tamaño fijo.
    /// </summary>
    public IEnumerable<Post[]> GetPostsInChunks(int chunkSize)
    {
        EnsureDataLoaded();
        
        return _posts!.Chunk(chunkSize);
    }

    #endregion

    #region 10. OPERADORES DE UNIÓN (Join, GroupJoin, Zip)

    /// <summary>
    /// JOIN: Une dos secuencias por una clave común (como INNER JOIN en SQL).
    /// </summary>
    public IEnumerable<PostWithAuthor> GetPostsWithAuthors()
    {
        EnsureDataLoaded();
        
        return _posts!.Join(
            inner: _users!,
            outerKeySelector: post => post.UserId,
            innerKeySelector: user => user.Id,
            resultSelector: (post, user) => new PostWithAuthor
            {
                PostId = post.Id,
                PostTitle = post.Title,
                AuthorId = user.Id,
                AuthorName = user.Name,
                AuthorEmail = user.Email
            }
        );
    }

    /// <summary>
    /// GROUPJOIN: Une con agrupación (como LEFT JOIN con GROUP BY).
    /// </summary>
    public IEnumerable<UserWithPosts> GetUsersWithTheirPosts()
    {
        EnsureDataLoaded();
        
        return _users!.GroupJoin(
            inner: _posts!,
            outerKeySelector: user => user.Id,
            innerKeySelector: post => post.UserId,
            resultSelector: (user, posts) => new UserWithPosts
            {
                UserId = user.Id,
                UserName = user.Name,
                Posts = posts.ToList(),
                PostCount = posts.Count()
            }
        );
    }

    /// <summary>
    /// ZIP: Combina dos secuencias elemento por elemento.
    /// </summary>
    public IEnumerable<string> ZipUsersWithPosts()
    {
        EnsureDataLoaded();
        
        // Zip combina el elemento N de la primera con el elemento N de la segunda
        return _users!.Zip(
            _posts!,
            (user, post) => $"{user.Name} escribió: {post.ShortTitle}"
        );
    }

    /// <summary>
    /// ZIP con tres secuencias (C# 10+).
    /// </summary>
    public IEnumerable<(User User, Post Post, Todo Todo)> ZipThreeCollections()
    {
        EnsureDataLoaded();
        
        return _users!.Zip(_posts!, _todos!);
    }

    #endregion

    #region 11. OPERADORES DE CONVERSIÓN (ToList, ToArray, ToDictionary, ToHashSet)

    /// <summary>
    /// Demuestra operadores de conversión.
    /// </summary>
    public ConversionExamples GetConversionExamples()
    {
        EnsureDataLoaded();
        
        return new ConversionExamples
        {
            // ToList: Convierte a List<T>
            UserList = _users!.Where(u => u.Id <= 3).ToList(),
            
            // ToArray: Convierte a T[]
            PostArray = _posts!.Take(5).ToArray(),
            
            // ToDictionary: Convierte a Dictionary<K, V>
            UserDictionary = _users!.ToDictionary(
                keySelector: u => u.Id,
                elementSelector: u => u.Name
            ),
            
            // ToHashSet: Convierte a HashSet<T> (sin duplicados)
            UniqueCities = _users!.Select(u => u.Address.City).ToHashSet(),
            
            // AsEnumerable: Fuerza evaluación como IEnumerable
            // (útil para cambiar de IQueryable a IEnumerable)
            EnumerableUsers = _users!.AsEnumerable()
        };
    }

    #endregion

    #region 12. CONSULTAS COMPLEJAS COMBINADAS

    /// <summary>
    /// Análisis completo combinando múltiples operadores LINQ.
    /// </summary>
    public ComprehensiveAnalysis GetComprehensiveAnalysis()
    {
        EnsureDataLoaded();
        
        // Usuarios más activos (más posts + todos completados)
        var mostActiveUsers = _users!
            .Select(u => new
            {
                User = u,
                PostCount = _posts!.Count(p => p.UserId == u.Id),
                CompletedTodos = _todos!.Count(t => t.UserId == u.Id && t.Completed),
                TotalTodos = _todos!.Count(t => t.UserId == u.Id)
            })
            .OrderByDescending(x => x.PostCount + x.CompletedTodos)
            .Take(5)
            .Select(x => new ActiveUserInfo
            {
                UserId = x.User.Id,
                UserName = x.User.Name,
                PostCount = x.PostCount,
                CompletedTodos = x.CompletedTodos,
                TodoCompletionRate = x.TotalTodos > 0 
                    ? (double)x.CompletedTodos / x.TotalTodos * 100 
                    : 0
            })
            .ToList();

        // Análisis de palabras más frecuentes en títulos
        var wordFrequency = _posts!
            .SelectMany(p => p.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(w => w.ToLowerInvariant().Trim('.', ',', '!', '?'))
            .Where(w => w.Length > 3)  // Solo palabras de más de 3 letras
            .GroupBy(w => w)
            .Select(g => new WordFrequency { Word = g.Key, Count = g.Count() })
            .OrderByDescending(wf => wf.Count)
            .Take(10)
            .ToList();

        // Distribución de todos por usuario y estado
        var todoDistribution = _users!
            .GroupJoin(
                _todos!,
                user => user.Id,
                todo => todo.UserId,
                (user, todos) => new TodoDistributionByUser
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    Completed = todos.Count(t => t.Completed),
                    Pending = todos.Count(t => !t.Completed),
                    HighPriority = todos.Count(t => t.Priority == TodoPriority.High)
                }
            )
            .Where(d => d.Completed + d.Pending > 0)
            .OrderByDescending(d => d.Pending)
            .ToList();

        return new ComprehensiveAnalysis
        {
            MostActiveUsers = mostActiveUsers,
            TopWords = wordFrequency,
            TodoDistribution = todoDistribution,
            TotalAnalyzedPosts = _posts!.Count,
            TotalAnalyzedTodos = _todos!.Count,
            OverallCompletionRate = _todos!.Count > 0 
                ? (double)_todos!.Count(t => t.Completed) / _todos!.Count * 100 
                : 0
        };
    }

    #endregion
}

#region DTOs para Analytics

public record UserSummaryDto
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string City { get; init; }
    public required string Company { get; init; }
    public int PostCount { get; init; }
    public int TodoCount { get; init; }
    public int CompletedTodoCount { get; init; }
    public double TodoCompletionRate => TodoCount > 0 ? (double)CompletedTodoCount / TodoCount * 100 : 0;
}

public record UserPostStats
{
    public int UserId { get; init; }
    public required string UserName { get; init; }
    public int PostCount { get; init; }
    public int TotalWords { get; init; }
    public double AverageWords { get; init; }
    public required Post LongestPost { get; init; }
}

public record TodoGroupStats
{
    public int UserId { get; init; }
    public bool IsCompleted { get; init; }
    public int Count { get; init; }
    public List<string> Titles { get; init; } = new();
}

public record AggregationStats
{
    public int TotalUsers { get; init; }
    public int TotalPosts { get; init; }
    public int TotalTodos { get; init; }
    public int CompletedTodos { get; init; }
    public int PendingTodos { get; init; }
    public int PostsWithLongTitles { get; init; }
}

public class WordAccumulator
{
    public int TotalPosts { get; set; }
    public int TotalWords { get; set; }
    public int MinWords { get; set; }
    public int MaxWords { get; set; }
}

public record WordStats
{
    public int TotalPosts { get; init; }
    public int TotalWords { get; init; }
    public double AverageWords { get; init; }
    public int MinWords { get; init; }
    public int MaxWords { get; init; }
}

public record QuantificationResults
{
    public bool HasUsers { get; init; }
    public bool HasUsersFromGwenborough { get; init; }
    public bool HasLongPosts { get; init; }
    public bool HasCompletedTodos { get; init; }
    public bool AllUsersHaveEmail { get; init; }
    public bool AllPostsHaveContent { get; init; }
    public bool AllTodosHaveTitle { get; init; }
    public bool ContainsUserId1 { get; init; }
    public bool ContainsUserId999 { get; init; }
}

public record PostWithAuthor
{
    public int PostId { get; init; }
    public required string PostTitle { get; init; }
    public int AuthorId { get; init; }
    public required string AuthorName { get; init; }
    public required string AuthorEmail { get; init; }
}

public record UserWithPosts
{
    public int UserId { get; init; }
    public required string UserName { get; init; }
    public List<Post> Posts { get; init; } = new();
    public int PostCount { get; init; }
}

public record ConversionExamples
{
    public List<User> UserList { get; init; } = new();
    public Post[] PostArray { get; init; } = Array.Empty<Post>();
    public Dictionary<int, string> UserDictionary { get; init; } = new();
    public HashSet<string> UniqueCities { get; init; } = new();
    public IEnumerable<User> EnumerableUsers { get; init; } = Enumerable.Empty<User>();
}

public record ActiveUserInfo
{
    public int UserId { get; init; }
    public required string UserName { get; init; }
    public int PostCount { get; init; }
    public int CompletedTodos { get; init; }
    public double TodoCompletionRate { get; init; }
}

public record WordFrequency
{
    public required string Word { get; init; }
    public int Count { get; init; }
}

public record TodoDistributionByUser
{
    public int UserId { get; init; }
    public required string UserName { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public int HighPriority { get; init; }
}

public record ComprehensiveAnalysis
{
    public List<ActiveUserInfo> MostActiveUsers { get; init; } = new();
    public List<WordFrequency> TopWords { get; init; } = new();
    public List<TodoDistributionByUser> TodoDistribution { get; init; } = new();
    public int TotalAnalyzedPosts { get; init; }
    public int TotalAnalyzedTodos { get; init; }
    public double OverallCompletionRate { get; init; }
}

#endregion