using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Services.Abstractions.DTOs.Candidates;

namespace Services.Mapping;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        // CandidateUser to CandidateProfileDto
        CreateMap<CandidateUser, CandidateProfileDto>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
            .ForMember(dest => dest.CareerLevel, opt => opt.MapFrom(src => src.CareerLevel.HasValue ? src.CareerLevel.Value.ToString() : null))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                (src.CandidateSkills ?? new List<CandidateSkill>()).Select(cs => cs.Skill)));

        // UpdateCandidateProfileDto to CandidateUser
        CreateMap<UpdateCandidateProfileDto, CandidateUser>()
            .ForMember(dest => dest.Gender, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.Gender) && Enum.TryParse<Gender>(src.Gender, true, out _));
                opt.MapFrom(src => Enum.Parse<Gender>(src.Gender!, true));
            })
            .ForMember(dest => dest.CareerLevel, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.CareerLevel) && Enum.TryParse<JobLevel>(src.CareerLevel, true, out _));
                opt.MapFrom(src => Enum.Parse<JobLevel>(src.CareerLevel!, true) as JobLevel?);
            })
            .ForMember(dest => dest.CandidateSkills, opt => opt.Ignore())
            .ForMember(dest => dest.Resumes, opt => opt.Ignore());

        // Resume to ResumeDto
        CreateMap<Resume, ResumeDto>()
            .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType));

        // Skill to SkillDto
        CreateMap<Skill, SkillDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.HasValue ? src.Category.Value.ToString() : null));
    }
}
