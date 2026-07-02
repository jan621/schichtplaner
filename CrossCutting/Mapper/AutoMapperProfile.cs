using AutoMapper;
using CrossCutting.Entities;
using CrossCutting.Entities.NotMapped;
using CrossCutting.Identity;

namespace CrossCutting.Mapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SignUpRequest, User>()
            .ForMember(dest => dest.UserName,
                opt =>
                    opt.MapFrom(src => src.Email))
            .ReverseMap();

        CreateMap<UserSession, User>().ReverseMap();
        
        CreateMap<SignUpEmployeeRequest, User>()
            .ForMember(dest => dest.UserName,
                opt =>
                    opt.MapFrom(src => src.Email))
            .ReverseMap();

        CreateMap<EditEmployeeRequest, User>().ReverseMap();
    }
}