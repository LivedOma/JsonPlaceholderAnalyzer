using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;

/// <summary>
/// DTO para deserializar posts desde la API.
/// </summary>
public record ApiPostDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body
);