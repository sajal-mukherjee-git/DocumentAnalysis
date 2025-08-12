using System.Diagnostics.Metrics;

namespace DocumentUploadAPI.Services;

public class DocumentUploadMetrics
{
    private readonly Meter _meter;
    private readonly Counter<int> _uploadCounter;
    private readonly Counter<int> _uploadErrorCounter;
    private readonly Histogram<double> _uploadDuration;
    private readonly Counter<long> _uploadSizeCounter;

    public DocumentUploadMetrics()
    {
        _meter = new Meter("DocumentUploadAPI.Metrics", "1.0.0");
        
        _uploadCounter = _meter.CreateCounter<int>(
            "document_uploads_total", 
            "count", 
            "Total number of document uploads");

        _uploadErrorCounter = _meter.CreateCounter<int>(
            "document_upload_errors_total", 
            "count", 
            "Total number of document upload errors");

        _uploadDuration = _meter.CreateHistogram<double>(
            "document_upload_duration", 
            "milliseconds", 
            "Duration of document upload operations");

        _uploadSizeCounter = _meter.CreateCounter<long>(
            "document_upload_size_bytes", 
            "bytes", 
            "Total size of uploaded documents in bytes");
    }

    public void RecordUpload(string fileType, long fileSize, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("file_type", fileType),
            new("status", "success")
        };

        _uploadCounter.Add(1, tags);
        _uploadSizeCounter.Add(fileSize, tags);
        _uploadDuration.Record(durationMs, tags);
    }

    public void RecordUploadError(string fileType, string errorType)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("file_type", fileType),
            new("error_type", errorType),
            new("status", "error")
        };

        _uploadErrorCounter.Add(1, tags);
    }

    public void RecordValidationError(string fileType, string validationError)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("file_type", fileType),
            new("validation_error", validationError),
            new("status", "validation_failed")
        };

        _uploadErrorCounter.Add(1, tags);
    }
}
