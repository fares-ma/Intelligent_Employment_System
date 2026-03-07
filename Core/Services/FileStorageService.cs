using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions;
using Shared.Configuration;

namespace Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    // Magic bytes for file validation
    private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46 }; // %PDF
    private static readonly byte[] DocxMagicBytes = { 0x50, 0x4B, 0x03, 0x04 }; // PK..
    private static readonly byte[] JpgMagicBytes = { 0xFF, 0xD8, 0xFF }; // JPG
    private static readonly byte[] PngMagicBytes = { 0x89, 0x50, 0x4E, 0x47 }; // PNG

    // Image extensions for profile pictures
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png" };

    public FileStorageService(IOptions<FileStorageSettings> options, ILogger<FileStorageService> logger)
    {
        _settings = options.Value;
        _logger = logger;

        // Ensure base path exists
        if (!Directory.Exists(_settings.BasePath))
        {
            Directory.CreateDirectory(_settings.BasePath);
        }
    }

    public async Task<string> SaveFileAsync(string fileName, Stream fileStream, string folder)
    {
        // Validation
        if (string.IsNullOrEmpty(fileName))
            throw new BadRequestException("File name is required");

        if (fileStream.Length > _settings.MaxFileSizeBytes)
            throw new BadRequestException($"File size exceeds {_settings.MaxFileSizeBytes / (1024 * 1024)}MB limit");

        var extension = Path.GetExtension(fileName).ToLower();
        
        // Determine allowed extensions based on folder
        bool isProfilePicture = folder.Contains("profile-picture", StringComparison.OrdinalIgnoreCase);
        IEnumerable<string> allowedExtensions = isProfilePicture ? ImageExtensions : _settings.AllowedExtensions;
        
        if (!allowedExtensions.Contains(extension))
            throw new BadRequestException($"File type {extension} not allowed. Allowed: {string.Join(", ", allowedExtensions)}");

        // Validate magic bytes
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[4];
        await fileStream.ReadAsync(buffer, 0, 4);
        fileStream.Seek(0, SeekOrigin.Begin);

        bool isValidFile = ValidateMagicBytes(extension, buffer);
        if (!isValidFile)
            throw new BadRequestException("File content does not match extension");

        // Generate random filename to prevent conflicts
        string filename = $"{Guid.NewGuid()}{extension}";
        string folderPath = Path.Combine(_settings.BasePath, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, filename);

        // Save file
        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file);
        }

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return filePath;
    }

    private bool ValidateMagicBytes(string extension, byte[] buffer)
    {
        return extension switch
        {
            ".pdf" => buffer.Take(4).SequenceEqual(PdfMagicBytes),
            ".docx" => buffer.Take(4).SequenceEqual(DocxMagicBytes),
            ".jpg" or ".jpeg" => buffer.Take(3).SequenceEqual(JpgMagicBytes),
            ".png" => buffer.Take(4).SequenceEqual(PngMagicBytes),
            _ => false
        };
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new NotFoundException("File not found");

        return await File.ReadAllBytesAsync(filePath);
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }
}
