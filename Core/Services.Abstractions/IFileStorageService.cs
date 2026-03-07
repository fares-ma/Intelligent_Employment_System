namespace Services.Abstractions;

public interface IFileStorageService
{
    /// <summary>
    /// Save a file to storage with validation
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="fileStream">File stream to save</param>
    /// <param name="folder">Subfolder within configured base path</param>
    /// <returns>Stored file path</returns>
    Task<string> SaveFileAsync(string fileName, Stream fileStream, string folder);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="filePath">Full path to delete</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Get file bytes for download
    /// </summary>
    /// <param name="filePath">Full path to file</param>
    /// <returns>File bytes</returns>
    Task<byte[]> GetFileAsync(string filePath);

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="filePath">Full path to file</param>
    bool FileExists(string filePath);
}
