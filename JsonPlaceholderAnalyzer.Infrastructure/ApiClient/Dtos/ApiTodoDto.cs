using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Infrastructure.ApiClient.Dtos;

/// <summary>
/// DTO para deserializar todos desde la API.
/// </summary>
public record ApiTodoDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("completed")] bool Completed
);