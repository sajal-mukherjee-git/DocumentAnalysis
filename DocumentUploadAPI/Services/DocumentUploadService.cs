using DocumentUploadAPI.Models;
using System.Diagnostics;

namespace DocumentUploadAPI.Services;

public interface IDocumentUploadService
{
    Task<UploadResponse> UploadDocumentAsync(UploadRequest request, CancellationToken cancellationToken = default);
}

public class DocumentUploadService : IDocumentUploadService
{
    private readonly IFileValidationService _validationService;
    private readonly IFileStorageService _storageService;
    private readonly DocumentUploadMetrics _metrics;
    private readonly ILogger<DocumentUploadService> _logger;

    public DocumentUploadService(
        IFileValidationService validationService,
        IFileStorageService storageService,
        DocumentUploadMetrics metrics,
        ILogger<DocumentUploadService> logger)
    {
        _validationService = validationService;
        _storageService = storageService;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<UploadResponse> UploadDocumentAsync(UploadRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        
        _logger.LogInformation("Starting document upload for file: {FileName}", request.File.FileName);

        try
        {
            // Validate the file
            var validationResult = _validationService.ValidateFile(request.File);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Errors);
                _logger.LogWarning("File validation failed for {FileName}: {Errors}", 
                    request.File.FileName, errorMessage);
                
                _metrics.RecordValidationError(fileExtension, errorMessage);
                throw new ValidationException($"File validation failed: {errorMessage}");
            }

            // Save the file
            var response = await _storageService.SaveFileAsync(request.File, request.Description, cancellationToken);
            
            stopwatch.Stop();
            _metrics.RecordUpload(fileExtension, request.File.Length, stopwatch.Elapsed.TotalMilliseconds);
            
            _logger.LogInformation("Successfully uploaded document {FileName} with ID {FileId} in {ElapsedMs}ms", 
                request.File.FileName, response.Id, stopwatch.Elapsed.TotalMilliseconds);
            
            return response;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordUploadError(fileExtension, ex.GetType().Name);
            
            _logger.LogError(ex, "Failed to upload document {FileName}", request.File.FileName);
            throw new InvalidOperationException("Failed to save the uploaded file.", ex);
        }
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
