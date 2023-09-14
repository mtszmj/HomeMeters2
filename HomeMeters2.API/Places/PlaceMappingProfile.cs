using AutoMapper;
using HomeMeters2.API.Places.Dtos;

namespace HomeMeters2.API.Places;

public class PlaceMappingProfile : Profile
{
    public PlaceMappingProfile()
    {
        CreateMap<Place, PlaceDto>()
            .ForMember(x => x.Id, 
                opt => opt.MapFrom((s,d) => string.Empty))
            ;
        
        CreateMap<Place, PlaceDeletedDto>()
            .ForMember(x => x.Id, 
                opt => opt.MapFrom((s,d) => string.Empty))
            ;
    }
    
    
}