using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Assessment;

namespace Services.Mapping;

public class AssessmentMappingProfile : Profile
{
    public AssessmentMappingProfile()
    {
        // Assessment mappings
        CreateMap<CreateAssessmentDto, Assessment>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Sum(q => q.Points) : 0))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
            
        CreateMap<UpdateAssessmentDto, Assessment>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Assessment, AssessmentDetailDto>();
        CreateMap<Assessment, AssessmentListDto>();
        
        CreateMap<Assessment, CandidateAssessmentAttemptDto>()
            .ForMember(dest => dest.CandidateAssessmentId, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeadlineAt, opt => opt.Ignore());

        // Question mappings
        CreateMap<CreateQuestionDto, Question>()
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
            
        CreateMap<UpdateQuestionDto, Question>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Question, AssessmentQuestionDto>();
    }
}
