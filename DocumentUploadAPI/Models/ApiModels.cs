namespace DocumentUploadAPI.Models;

public record UploadRequest(IFormFile File, string? Description = null);

public record UploadResponse(
    string Id,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    DateTime UploadedAt,
    string StoragePath
);

public record ValidationResult(bool IsValid, IEnumerable<string> Errors);

public record ApiError(string Message, string? Details = null, int StatusCode = 400);

public class FileUploadOptions
{
    public const string SectionName = "FileUpload";
    
    public string StoragePath { get; set; } = "/app/uploads";
    public string TempPath { get; set; } = "/app/temp";
    public long MaxFileSizeBytes { get; set; } = 104857600; // 100MB
    public string[] AllowedExtensions { get; set; } = [".pdf", ".docx", ".doc", ".txt"];
}

public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    public string Title { get; set; } = "Document Upload API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "A minimal API for document upload operations";
}

public class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";
    
    public string ServiceName { get; set; } = "DocumentUploadAPI";
    public string ServiceVersion { get; set; } = "1.0.0";
    public bool EnableConsoleExporter { get; set; } = true;
    public bool EnableFileExporter { get; set; } = false;
    public string LogDirectory { get; set; } = "./logs";
}
