using Domain.Contracts;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Configuration;
using Services.Abstractions;

namespace Services;

public class RecruiterService : IRecruiterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<RecruiterService> _logger;
    private readonly IOptions<FileStorageSettings> _fileStorageSettings;

    public RecruiterService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<RecruiterService> logger,
        IOptions<FileStorageSettings> fileStorageSettings)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _fileStorageSettings = fileStorageSettings;
    }

    private string ToRelativeStoragePath(string absolutePath)
    {
        var baseFull = Path.GetFullPath(_fileStorageSettings.Value.BasePath);
        var full = Path.GetFullPath(absolutePath);
        if (!full.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
            return absolutePath;
        var rel = Path.GetRelativePath(baseFull, full);
        return rel.Replace('\\', '/');
    }

    public async Task<string> UpdateProfilePictureAsync(string recruiterId, string fileName, Stream fileStream)
    {
        var recruiter = await _unitOfWork.Recruiters.GetByIdAsync(recruiterId);
        if (recruiter is null)
            throw new NotFoundException("Recruiter not found");

        if (!string.IsNullOrWhiteSpace(recruiter.ProfilePicturePath))
        {
            await _fileStorageService.DeleteFileAsync(recruiter.ProfilePicturePath);
        }

        var newPath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "profile-pictures");
        recruiter.ProfilePicturePath = ToRelativeStoragePath(newPath);
        recruiter.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Recruiters.Update(recruiter);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile picture updated for recruiter {RecruiterId}", recruiterId);

        return recruiter.ProfilePicturePath;
    }
}
