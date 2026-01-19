using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;

/// <summary>
/// DTO para deserializar fotos desde la API.
/// </summary>
public record ApiPhotoDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("albumId")] int AlbumId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("thumbnailUrl")] string ThumbnailUrl
);