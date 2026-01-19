using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Application.DTOs;

public record ApiCommentDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("postId")] int PostId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("body")] string Body
);