using DocumentUploadAPI.Models;

namespace DocumentUploadAPI.Services;

public interface IFileStorageService
{
    Task<UploadResponse> SaveFileAsync(IFormFile file, string? description = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(FileUploadOptions options, ILogger<LocalFileStorageService> logger)
    {
        _options = options;
        _logger = logger;
        
        // Ensure directories exist
        EnsureDirectoriesExist();
    }

    public async Task<UploadResponse> SaveFileAsync(IFormFile file, string? description = null, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString("N");
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{fileId}{fileExtension}";
        var filePath = Path.Combine(_options.StoragePath, uniqueFileName);

        try
        {
            _logger.LogInformation("Saving file {OriginalFileName} as {UniqueFileName}", 
                file.FileName, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var response = new UploadResponse(
                Id: fileId,
                FileName: uniqueFileName,
                OriginalFileName: file.FileName,
                FileSize: file.Length,
                ContentType: file.ContentType,
                UploadedAt: DateTime.UtcNow,
                StoragePath: filePath
            );

            _logger.LogInformation("Successfully saved file {UniqueFileName} with ID {FileId}", 
                uniqueFileName, fileId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file {OriginalFileName}", file.FileName);
            
            // Clean up partial file if it exists
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to clean up partial file {FilePath}", filePath);
                }
            }
            
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_options.StoragePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted file {FileName}", fileName);
                return Task.FromResult(true);
            }
            
            _logger.LogWarning("File {FileName} not found for deletion", fileName);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileName}", fileName);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_options.StoragePath, fileName);
        return Task.FromResult(File.Exists(filePath));
    }

    private void EnsureDirectoriesExist()
    {
        try
        {
            if (!Directory.Exists(_options.StoragePath))
            {
                Directory.CreateDirectory(_options.StoragePath);
                _logger.LogInformation("Created storage directory: {StoragePath}", _options.StoragePath);
            }

            if (!Directory.Exists(_options.TempPath))
            {
                Directory.CreateDirectory(_options.TempPath);
                _logger.LogInformation("Created temp directory: {TempPath}", _options.TempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create required directories");
            throw;
        }
    }
}
