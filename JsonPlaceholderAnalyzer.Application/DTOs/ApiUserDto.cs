using System.Text.Json.Serialization;

namespace JsonPlaceholderAnalyzer.Application.DTOs;

public record ApiUserDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("address")] ApiAddressDto Address,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("website")] string? Website,
    [property: JsonPropertyName("company")] ApiCompanyDto Company
);

public record ApiAddressDto(
    [property: JsonPropertyName("street")] string Street,
    [property: JsonPropertyName("suite")] string Suite,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("zipcode")] string Zipcode,
    [property: JsonPropertyName("geo")] ApiGeoDto Geo
);

public record ApiGeoDto(
    [property: JsonPropertyName("lat")] string Lat,
    [property: JsonPropertyName("lng")] string Lng
);

public record ApiCompanyDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("catchPhrase")] string CatchPhrase,
    [property: JsonPropertyName("bs")] string Bs
);