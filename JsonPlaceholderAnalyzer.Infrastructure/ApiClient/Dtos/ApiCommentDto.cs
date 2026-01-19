using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;

/// <summary>
/// DTO para deserializar comentarios desde la API.
/// </summary>
public record ApiCommentDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("postId")] int PostId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("body")] string Body
);