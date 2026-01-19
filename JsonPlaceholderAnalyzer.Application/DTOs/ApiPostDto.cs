using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Application.DTOs;

public record ApiPostDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body
);