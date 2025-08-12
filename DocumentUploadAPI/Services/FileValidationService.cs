using DocumentUploadAPI.Models;

namespace DocumentUploadAPI.Services;

public interface IFileValidationService
{
    ValidationResult ValidateFile(IFormFile file);
}

public class FileValidationService : IFileValidationService
{
    private readonly FileUploadOptions _options;
    private readonly ILogger<FileValidationService> _logger;

    public FileValidationService(FileUploadOptions options, ILogger<FileValidationService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public ValidationResult ValidateFile(IFormFile file)
    {
        var errors = new List<string>();

        if (file == null || file.Length == 0)
        {
            errors.Add("File is required and cannot be empty.");
            return new ValidationResult(false, errors);
        }

        // Check file size
        if (file.Length > _options.MaxFileSizeBytes)
        {
            errors.Add($"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / 1024 / 1024} MB.");
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(fileExtension))
        {
            errors.Add($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", _options.AllowedExtensions)}");
        }

        // Check for potentially dangerous file names
        if (string.IsNullOrWhiteSpace(file.FileName) || 
            file.FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            errors.Add("Invalid file name.");
        }

        var isValid = !errors.Any();
        
        _logger.LogInformation("File validation result for {FileName}: {IsValid}", 
            file.FileName, isValid);

        return new ValidationResult(isValid, errors);
    }
}
