using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Auth;

namespace Services.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // RegisterRequestDto mapping
        CreateMap<RegisterRequestDto, CandidateUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        CreateMap<RegisterRequestDto, Recruiter>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // LoginRequestDto (no mapping needed, used directly in service)

        // LoginResponseDto (mapped from ApplicationUser in service)
    }
}
