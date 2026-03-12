using AutoMapper;
using Domain.Models;
using Domain.Enums;
using Services.Abstractions.DTOs.Auth;
using Services.Abstractions.DTOs.Company;

namespace Services.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // RegisterRequestDto mapping (Candidate or old-style Recruiter)
        CreateMap<RegisterRequestDto, CandidateUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        CreateMap<RegisterRequestDto, Recruiter>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // RegisterCompanyRequestDto → Company and Recruiter (Admin)
        CreateMap<RegisterCompanyRequestDto, Company>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        CreateMap<RegisterCompanyRequestDto, Recruiter>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RecruiterRole, opt => opt.MapFrom(_ => UserRole.Admin))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // RegisterRecruiterRequestDto → Recruiter (Standard)
        CreateMap<RegisterRecruiterRequestDto, Recruiter>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RecruiterRole, opt => opt.MapFrom(_ => UserRole.Standard))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // UpdateCompanyDto → Company
        CreateMap<UpdateCompanyDto, Company>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // LoginRequestDto (no mapping needed, used directly in service)

        // LoginResponseDto (mapped from ApplicationUser in service)
    }
}
