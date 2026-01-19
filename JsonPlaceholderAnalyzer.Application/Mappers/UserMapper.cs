using JsonPlaceholderAnalyzer.Application.Interfaces;
using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;

namespace JsonPlaceholderAnalyzer.Application.Mappers;

public class UserMapper : IMapper<ApiUserDto, User>
{
    public User Map(ApiUserDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        return new User
        {
            Id = source.Id,
            Name = source.Name,
            Username = source.Username,
            Email = source.Email,
            Phone = source.Phone,
            Website = source.Website,
            Address = MapAddress(source.Address),
            Company = MapCompany(source.Company)
        };
    }
    
    private static Address MapAddress(ApiAddressDto dto) => new(
        Street: dto.Street,
        Suite: dto.Suite,
        City: dto.City,
        Zipcode: dto.Zipcode,
        Geo: new GeoLocation(dto.Geo.Lat, dto.Geo.Lng)
    );
    
    private static Company MapCompany(ApiCompanyDto dto) => new(
        Name: dto.Name,
        CatchPhrase: dto.CatchPhrase,
        Bs: dto.Bs
    );
}