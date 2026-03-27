using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Assessment;

namespace Services.Mapping;

public class AssessmentMappingProfile : Profile
{
    public AssessmentMappingProfile()
    {
        CreateMap<Assessment, AssessmentDto>().ReverseMap();
        CreateMap<Question, QuestionDto>().ReverseMap();
        
        CreateMap<CreateAssessmentDto, Assessment>();
        CreateMap<CreateQuestionDto, Question>();

        CreateMap<CandidateAssessment, CandidateAssessmentDto>();
    }
}
