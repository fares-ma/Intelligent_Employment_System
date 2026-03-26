using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Interview;

namespace Services.Mapping;

public class InterviewMappingProfile : Profile
{
    public InterviewMappingProfile()
    {
        CreateMap<Interview, InterviewDto>()
            .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
            .ForMember(dest => dest.CandidateName, opt => opt.Ignore())
            .ForMember(dest => dest.JobTitle, opt => opt.Ignore());

        CreateMap<InterviewDto, Interview>()
            .ForMember(dest => dest.JobApplication, opt => opt.Ignore());

        CreateMap<CreateInterviewDto, Interview>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.JobApplication, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Domain.Enums.InterviewStatus.Scheduled))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<UpdateInterviewDto, Interview>()
            .ForMember(dest => dest.InterviewType, opt => opt.Ignore())
            .ForMember(dest => dest.JobApplicationId, opt => opt.Ignore())
            .ForMember(dest => dest.JobApplication, opt => opt.Ignore())
            .ForMember(dest => dest.ScheduledAt, opt => opt.Ignore())
            .ForMember(dest => dest.DurationMinutes, opt => opt.Ignore())
            .ForMember(dest => dest.AiQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
