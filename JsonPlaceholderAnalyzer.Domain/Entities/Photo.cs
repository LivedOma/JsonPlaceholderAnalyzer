namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa una foto de JSONPlaceholder.
/// Demuestra: Uri como tipo, propiedades con valores por defecto.
/// </summary>
public class Photo : EntityBase<int>
{
    public required int AlbumId { get; init; }
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string ThumbnailUrl { get; init; }
    
    // Propiedades calculadas
    public Uri? UrlAsUri => Uri.TryCreate(Url, UriKind.Absolute, out var uri) ? uri : null;
    public Uri? ThumbnailAsUri => Uri.TryCreate(ThumbnailUrl, UriKind.Absolute, out var uri) ? uri : null;
    public bool HasValidUrls => UrlAsUri is not null && ThumbnailAsUri is not null;
}