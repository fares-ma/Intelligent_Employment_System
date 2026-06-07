using System.Text.Json;
using Domain.Models;
using Services.Abstractions.DTOs.JobPosting;
using Services.Abstractions.TalentX;

namespace Services.TalentX;

/// <summary>
/// Maps JobPost + skills into TalentX job_description JSON schema.
/// Uses new structured fields (DegreesJson, RolesJson, SkillsJson, GPA, Experience) when available.
/// </summary>
public class JobDescriptionBuilder : IJobDescriptionBuilder
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public string BuildJson(JobPost jobPost)
    {
        var degrees = DeserializeJson<List<JobDegreeDto>>(jobPost.DegreesJson) ?? new();
        var roles = DeserializeJson<List<JobRoleDto>>(jobPost.RolesJson) ?? new();
        var skillsDtos = DeserializeJson<List<JobSkillDto>>(jobPost.SkillsJson) ?? new();

        // Fall back to JobPostSkills if no SkillsJson
        var mustHaveSkills = skillsDtos.Any()
            ? skillsDtos
                .Where(s => !string.Equals(s.SkillPriority, "None", StringComparison.OrdinalIgnoreCase))
                .Select(s => new { name = s.SkillName, priority = s.SkillPriority })
                .ToList()
            : jobPost.JobPostSkills
                .Where(jps => jps.IsRequired)
                .Select(jps => new { name = jps.Skill.Name, priority = "High" })
                .ToList();

        var niceToHaveSkills = skillsDtos.Any()
            ? skillsDtos
                .Where(s => string.Equals(s.SkillPriority, "None", StringComparison.OrdinalIgnoreCase))
                .Select(s => new { name = s.SkillName, priority = "Low" })
                .ToList()
            : jobPost.JobPostSkills
                .Where(jps => !jps.IsRequired)
                .Select(jps => new { name = jps.Skill.Name, priority = "Low" })
                .ToList();

        var expMin = jobPost.ExperienceMinYears ?? MapExperienceYears(jobPost).min;
        var expMax = jobPost.ExperienceMaxYears ?? MapExperienceYears(jobPost).max;
        var expPriority = jobPost.ExperiencePriority ?? "Low";

        var requiredDegrees = degrees
            .Select(d => new { name = d.DegreeName, priority = d.DegreePriority })
            .ToList();

        var requiredRoles = roles
            .Select(r => new { name = r.RoleName, priority = r.RolePriority })
            .ToList();

        var payload = new
        {
            job_title = jobPost.Title,
            department = jobPost.Department,
            education = new
            {
                description = Truncate(jobPost.Description, 500),
                priority = jobPost.GPAPriority ?? "High",
                required_degrees = requiredDegrees,
                gpa = new
                {
                    min = jobPost.GPA ?? 0.0m,
                    priority = jobPost.GPAPriority ?? "Low"
                }
            },
            experience = new
            {
                description = $"Career level: {jobPost.CareerLevel}. {Truncate(jobPost.Description, 400)}",
                priority = expPriority,
                roles = requiredRoles,
                years = new { min = expMin, max = expMax, priority = expPriority }
            },
            skills = new
            {
                description = "Required technical skills for this role.",
                priority = "High",
                must_have = mustHaveSkills,
                nice_to_have = niceToHaveSkills
            }
        };

        return JsonSerializer.Serialize(payload, TalentXJsonSerializerOptions.Default);
    }

    private static (int min, int max) MapExperienceYears(JobPost jobPost) => jobPost.CareerLevel switch
    {
        Domain.Enums.JobLevel.Entry => (0, 1),
        Domain.Enums.JobLevel.Junior => (0, 2),
        Domain.Enums.JobLevel.Mid => (2, 5),
        Domain.Enums.JobLevel.Senior => (5, 15),
        Domain.Enums.JobLevel.Lead => (7, 20),
        Domain.Enums.JobLevel.Manager => (8, 20),
        Domain.Enums.JobLevel.Director => (10, 25),
        Domain.Enums.JobLevel.Executive => (12, 30),
        _ => (0, 5)
    };

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static T? DeserializeJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
