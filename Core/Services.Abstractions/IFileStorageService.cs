namespace Services.Abstractions;

public interface IFileStorageService
{
    /// <summary>
    /// Save a file to storage with validation
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="fileStream">File stream to save</param>
    /// <param name="folder">Subfolder within configured base path</param>
    /// <returns>Stored file path (relative to base path)</returns>
    Task<string> SaveFileAsync(string fileName, Stream fileStream, string folder);

    /// <summary>
    /// Delete a file from storage. Path must resolve within the storage root.
    /// </summary>
    /// <param name="filePath">Path to delete (relative to storage root)</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Get a readable stream for a file. Caller is responsible for disposing the returned Stream.
    /// </summary>
    /// <param name="filePath">Path to file (relative to storage root)</param>
    /// <returns>A readable Stream. Caller must dispose.</returns>
    Task<Stream> GetFileStreamAsync(string filePath);

    /// <summary>
    /// Check if file exists within the storage root.
    /// </summary>
    /// <param name="filePath">Path to file (relative to storage root)</param>
    bool FileExists(string filePath);
}
