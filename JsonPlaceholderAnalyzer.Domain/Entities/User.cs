namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa un usuario de JSONPlaceholder.
/// Demuestra: herencia, records anidados, required members, init-only.
/// </summary>
public class User : EntityBase<int>
{
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public required Company Company { get; init; }
    
    // Propiedad calculada
    public string DisplayName => $"{Name} (@{Username})";
}

/// <summary>
/// Record para la dirección del usuario.
/// Demuestra: records, inmutabilidad, with-expressions capability.
/// </summary>
public record Address(
    string Street,
    string Suite,
    string City,
    string Zipcode,
    GeoLocation Geo
)
{
    public string FullAddress => $"{Street}, {Suite}, {City} {Zipcode}";
}

/// <summary>
/// Record para geolocalización.
/// Demuestra: records simples, propiedades calculadas en records.
/// </summary>
public record GeoLocation(string Lat, string Lng)
{
    public (double Latitude, double Longitude)? Coordinates
    {
        get
        {
            if (double.TryParse(Lat, out var lat) && double.TryParse(Lng, out var lng))
                return (lat, lng);
            return null;
        }
    }
}

/// <summary>
/// Record para la compañía del usuario.
/// </summary>
public record Company(string Name, string CatchPhrase, string Bs);