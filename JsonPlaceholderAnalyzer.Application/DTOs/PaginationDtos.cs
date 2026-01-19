namespace JsonPlaceholderAnalyzer.Application.DTOs;

/// <summary>
/// DTOs para paginación y filtrado.
/// 
/// Demuestra:
/// - Records con valores por defecto
/// - Deconstrucción
/// - Validación en propiedades calculadas
/// </summary>

/// <summary>
/// Request para paginación.
/// </summary>
public record PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    // Validación mediante propiedades calculadas
    public int ValidPage => Page < 1 ? 1 : Page;
    public int ValidPageSize => PageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => PageSize
    };
    
    public int Skip => (ValidPage - 1) * ValidPageSize;
    
    /// <summary>
    /// Deconstructor para usar en pattern matching.
    /// Permite: var (page, size) = paginationRequest;
    /// </summary>
    public void Deconstruct(out int page, out int pageSize)
    {
        page = ValidPage;
        pageSize = ValidPageSize;
    }
}

/// <summary>
/// Respuesta paginada genérica.
/// </summary>
public record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalItems { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    
    // Propiedades calculadas
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public int ItemCount => Items.Count;
    
    /// <summary>
    /// Crea una respuesta paginada a partir de una colección.
    /// </summary>
    public static PaginatedResponse<T> Create(
        IEnumerable<T> allItems, 
        PaginationRequest request)
    {
        var (page, pageSize) = request; // Usando deconstrucción
        var items = allItems.ToList();
        
        return new PaginatedResponse<T>
        {
            Items = items.Skip(request.Skip).Take(pageSize).ToList(),
            TotalItems = items.Count,
            Page = page,
            PageSize = pageSize
        };
    }
}

/// <summary>
/// Request para ordenamiento.
/// </summary>
public record SortRequest
{
    public string? SortBy { get; init; }
    public bool Descending { get; init; } = false;
    
    public string SortDirection => Descending ? "DESC" : "ASC";
    
    public void Deconstruct(out string? sortBy, out bool descending)
    {
        sortBy = SortBy;
        descending = Descending;
    }
}

/// <summary>
/// Request combinado para consultas.
/// </summary>
public record QueryRequest
{
    public PaginationRequest Pagination { get; init; } = new();
    public SortRequest Sort { get; init; } = new();
    public string? SearchTerm { get; init; }
    
    public bool HasSearch => !string.IsNullOrWhiteSpace(SearchTerm);
    public bool HasSort => !string.IsNullOrWhiteSpace(Sort.SortBy);
}