namespace Services.Abstractions.TalentX.Models;

public class TalentXScoreRequest
{
  public int CandidateId { get; set; }
  public int JobId { get; set; }
  public required Stream CvFileStream { get; set; }
  public required string CvFileName { get; set; }
  public required string JobDescriptionJson { get; set; }
}
