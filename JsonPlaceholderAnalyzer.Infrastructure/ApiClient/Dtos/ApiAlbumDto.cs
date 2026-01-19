using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;

/// <summary>
/// DTO para deserializar álbumes desde la API.
/// </summary>
public record ApiAlbumDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title
);