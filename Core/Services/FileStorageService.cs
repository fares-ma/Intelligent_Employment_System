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
    private readonly string _basePathFull;

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

        // Canonicalize base path once
        _basePathFull = Path.GetFullPath(_settings.BasePath);

        // Ensure base path exists
        if (!Directory.Exists(_basePathFull))
        {
            Directory.CreateDirectory(_basePathFull);
        }
    }

    public async Task<string> SaveFileAsync(string fileName, Stream fileStream, string folder)
    {
        // --- Null / empty guards ---
        if (string.IsNullOrEmpty(fileName))
            throw new BadRequestException("File name is required");

        if (fileStream == null)
            throw new BadRequestException("File stream is required");

        if (string.IsNullOrWhiteSpace(folder))
            throw new BadRequestException("Storage folder is required");

        // --- Buffer non-seekable streams so we can check length & magic bytes ---
        Stream workingStream = fileStream;
        MemoryStream? buffered = null;

        if (!fileStream.CanSeek)
        {
            buffered = new MemoryStream();
            await fileStream.CopyToAsync(buffered);
            buffered.Seek(0, SeekOrigin.Begin);
            workingStream = buffered;
        }

        try
        {
            // --- Size check ---
            if (workingStream.Length > _settings.MaxFileSizeBytes)
                throw new BadRequestException($"File size exceeds {_settings.MaxFileSizeBytes / (1024 * 1024)}MB limit");

            var extension = Path.GetExtension(fileName).ToLower();

            // Determine allowed extensions based on folder
            bool isProfilePicture = folder.Contains("profile-picture", StringComparison.OrdinalIgnoreCase);
            IEnumerable<string> allowedExtensions = isProfilePicture ? ImageExtensions : _settings.AllowedExtensions;

            if (!allowedExtensions.Contains(extension))
                throw new BadRequestException($"File type {extension} not allowed. Allowed: {string.Join(", ", allowedExtensions)}");

            // --- Validate magic bytes ---
            workingStream.Seek(0, SeekOrigin.Begin);
            byte[] magicBuffer = new byte[4];
            int bytesRead = await workingStream.ReadAsync(magicBuffer.AsMemory(0, 4));
            workingStream.Seek(0, SeekOrigin.Begin);

            if (bytesRead < 3 || !ValidateMagicBytes(extension, magicBuffer))
                throw new BadRequestException("File content does not match extension");

            // --- Path traversal protection ---
            string sanitizedFolder = SanitizePath(folder);
            string folderPath = Path.GetFullPath(Path.Combine(_basePathFull, sanitizedFolder));

            if (!folderPath.StartsWith(_basePathFull, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Invalid storage folder");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Generate random filename to prevent conflicts
            string storedFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(folderPath, storedFileName);

            // Save file
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await workingStream.CopyToAsync(file);
            }

            _logger.LogInformation("File saved: {FilePath}", filePath);
            return filePath;
        }
        finally
        {
            if (buffered != null)
                await buffered.DisposeAsync();
        }
    }

    private static bool ValidateMagicBytes(string extension, byte[] buffer)
    {
        return extension switch
        {
            ".pdf" => buffer.Take(4).SequenceEqual(PdfMagicBytes),
            ".docx" => buffer.Take(4).SequenceEqual(DocxMagicBytes),
            ".jpg" or ".jpeg" => buffer.Take(3).SequenceEqual(JpgMagicBytes),
            ".png" => buffer.Take(4).SequenceEqual(PngMagicBytes),
            _ => true // Allow extensions with no known magic-byte signature
        };
    }

    public async Task DeleteFileAsync(string filePath)
    {
        string resolvedPath = ResolveSafePath(filePath);

        try
        {
            if (File.Exists(resolvedPath))
            {
                await Task.Run(() => File.Delete(resolvedPath));
                _logger.LogInformation("File deleted: {FilePath}", resolvedPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", resolvedPath);
            throw;
        }
    }

    public Task<Stream> GetFileStreamAsync(string filePath)
    {
        string resolvedPath = ResolveSafePath(filePath);

        if (!File.Exists(resolvedPath))
            throw new NotFoundException("File not found");

        Stream stream = new FileStream(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public bool FileExists(string filePath)
    {
        try
        {
            string resolvedPath = ResolveSafePath(filePath);
            return File.Exists(resolvedPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves a file path and ensures it is within the storage root.
    /// Prevents path traversal attacks.
    /// </summary>
    private string ResolveSafePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new BadRequestException("File path is required");

        string fullPath = Path.GetFullPath(filePath);

        if (!fullPath.StartsWith(_basePathFull, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt blocked: {FilePath}", filePath);
            throw new ForbiddenException("Access to the specified path is denied");
        }

        return fullPath;
    }

    /// <summary>
    /// Sanitizes folder path by removing dangerous segments.
    /// </summary>
    private static string SanitizePath(string folder)
    {
        // Reject any path with invalid chars
        if (folder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new BadRequestException("Folder name contains invalid characters");

        // Reject path traversal
        string normalized = folder.Replace('\\', '/');
        if (normalized.Contains(".."))
            throw new BadRequestException("Folder name cannot contain path traversal sequences");

        return folder;
    }
}
