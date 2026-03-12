namespace Shared.Configuration;

public class FileStorageSettings
{
    public string BasePath { get; set; } = "Uploads";
    public long MaxFileSizeBytes { get; set; } = 10485760; // 10MB
    public List<string> AllowedExtensions { get; set; } = new() { ".pdf", ".docx" };
}
