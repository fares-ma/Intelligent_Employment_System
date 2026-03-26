using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.JobPosting;
using Services.Abstractions.DTOs.JobApplication;

namespace Services.Mapping;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        // JobPost ↔ JobPostingDto
        CreateMap<JobPost, JobPostingDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
            .ForMember(dest => dest.RequiredSkills, opt => opt.MapFrom(src => 
                (src.JobPostSkills ?? new List<JobPostSkill>()).Select(js => js.Skill.Name)))
            .ForMember(dest => dest.ApplicationCount, opt => opt.MapFrom(src => 
                src.JobApplications != null ? src.JobApplications.Count : 0))
            .ForMember(dest => dest.ApplicationDeadline, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.JobType.ToString()));

        // CreateJobPostingDto → JobPost
        CreateMap<CreateJobPostingDto, JobPost>()
            .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.JobType>(src.EmploymentType, true)))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ApplicationDeadline))
            .ForMember(dest => dest.JobPostSkills, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();

        // UpdateJobPostingDto → JobPost
        CreateMap<UpdateJobPostingDto, JobPost>()
            .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => Enum.Parse<Domain.Enums.JobType>(src.EmploymentType, true)))
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ApplicationDeadline))
            .ForMember(dest => dest.JobPostSkills, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // JobApplication → JobApplicationDto
        CreateMap<JobApplication, JobApplicationDto>()
            .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Candidate != null ? $"{src.Candidate.FirstName} {src.Candidate.LastName}" : string.Empty))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPost != null ? src.JobPost.Title : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RecruiterNotes));
            
        // CreateJobApplicationDto → JobApplication
        CreateMap<CreateJobApplicationDto, JobApplication>()
            .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Domain.Enums.ApplicationStatus.Pending))
            .IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
