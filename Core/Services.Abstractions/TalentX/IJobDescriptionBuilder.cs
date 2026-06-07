using Domain.Models;

namespace Services.Abstractions.TalentX;

/// <summary>
/// Builds TalentX job_description JSON from a JobPost aggregate.
/// </summary>
public interface IJobDescriptionBuilder
{
  string BuildJson(JobPost jobPost);
}
