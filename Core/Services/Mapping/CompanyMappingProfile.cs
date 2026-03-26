using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Company;
using Services.Abstractions.DTOs.InviteCode;

namespace Services.Mapping;

public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        // Company to CompanyProfileDto
        CreateMap<Company, CompanyProfileDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ActiveJobPostsCount, opt => opt.MapFrom(src => src.JobPosts.Count(jp => jp.DeletedAt == null)))
            .ForMember(dest => dest.RecruiterCount, opt => opt.MapFrom(src => src.Recruiters.Count))
            // Admin metadata needs to be mapped differently or ignored if done manually in service
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminName, opt => opt.Ignore());

        // CompanyInviteCode to InviteCodeDto
        CreateMap<CompanyInviteCode, InviteCodeDto>();
    }
}
