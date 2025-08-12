# OpenTelemetry and Application Insights Integration

## Overview

The Document Upload API has been enhanced with OpenTelemetry for comprehensive observability, including:

- **Distributed Tracing**: Track requests across service boundaries
- **Metrics Collection**: Custom and system metrics for performance monitoring
- **Logging**: Structured logging with OpenTelemetry
- **Application Insights Integration**: Azure Monitor support for production environments

## Configuration

### Application Insights (Production)

For production environments with Azure Application Insights:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
  }
}
```

### OpenTelemetry Configuration

```json
{
  "OpenTelemetry": {
    "ServiceName": "DocumentUploadAPI",
    "ServiceVersion": "1.0.0",
    "EnableConsoleExporter": true,
    "EnableFileExporter": false,
    "LogDirectory": "/app/logs"
  }
}
```

## Metrics Collected

### Custom Metrics

- **document_uploads_total**: Total number of successful uploads
- **document_upload_errors_total**: Total number of failed uploads
- **document_upload_duration**: Duration of upload operations (histogram)
- **document_upload_size_bytes**: Total bytes uploaded

### System Metrics

- **ASP.NET Core metrics**: Request rate, duration, status codes
- **HTTP client metrics**: Outbound HTTP calls
- **Runtime metrics**: GC, memory, thread pool statistics

### Metric Tags

All custom metrics include contextual tags:
- `file_type`: File extension (.pdf, .docx, etc.)
- `status`: success, error, validation_failed
- `error_type`: Exception type for failed uploads
- `validation_error`: Specific validation error details

## Tracing

### Automatic Instrumentation

- **ASP.NET Core**: HTTP requests, middleware pipeline
- **HTTP Client**: Outbound HTTP calls
- **Exceptions**: Automatic exception recording

### Custom Spans

The upload service creates custom spans for:
- File validation operations
- File storage operations
- Business logic execution

### Trace Filtering

Health checks and Swagger endpoints are excluded from tracing to reduce noise.

## Local Development

### Console Output

When running locally, OpenTelemetry exports traces and metrics to the console:

```bash
dotnet run
```

You'll see trace information like:
```
Activity.TraceId:            80000000-0000-0000-0000-000000000000
Activity.SpanId:             8000000000000000
Activity.DisplayName:        POST /api/documents/upload
Activity.Kind:               Server
Activity.StartTime:          2025-08-01T13:45:00.0000000Z
Activity.Duration:           00:00:00.1234567
```

### Metrics Output

Metrics are displayed periodically:
```
Metric: document_uploads_total
Value: 5
Tags: file_type=pdf, status=success

Metric: document_upload_duration
Value: 123.45 ms
Tags: file_type=pdf, status=success
```

## Production Deployment

### Environment Variables

Set the Application Insights connection string:

```bash
# Docker
docker run -e "ApplicationInsights__ConnectionString=your-connection-string" document-upload-api

# Kubernetes
env:
- name: ApplicationInsights__ConnectionString
  value: "your-connection-string"
```

### Azure Application Insights

When deployed with a valid Application Insights connection string:

1. **Distributed Traces**: View request flows in Application Insights
2. **Custom Metrics**: Monitor upload rates and performance
3. **Live Metrics**: Real-time performance monitoring
4. **Alerts**: Set up alerts on custom metrics
5. **Dashboards**: Create custom dashboards for business metrics

## Monitoring Queries

### KQL Queries for Application Insights

**Upload Success Rate**:
```kql
customMetrics
| where name == "document_uploads_total"
| summarize SuccessfulUploads = sum(value) by bin(timestamp, 5m)
```

**Average Upload Duration**:
```kql
customMetrics
| where name == "document_upload_duration"
| summarize AvgDuration = avg(value) by bin(timestamp, 5m)
```

**Error Rate by File Type**:
```kql
customMetrics
| where name == "document_upload_errors_total"
| extend FileType = tostring(customDimensions.file_type)
| summarize ErrorCount = sum(value) by FileType, bin(timestamp, 1h)
```

**Request Traces**:
```kql
requests
| where url contains "/api/documents/upload"
| summarize RequestCount = count(), AvgDuration = avg(duration) by bin(timestamp, 5m)
```

## Troubleshooting

### Common Issues

1. **Missing Connection String**: App runs locally but no data in Application Insights
   - Verify `ApplicationInsights:ConnectionString` is set
   - Check connection string format

2. **No Custom Metrics**: System metrics work but custom metrics missing
   - Verify `DocumentUploadMetrics` is registered as singleton
   - Check meter name matches configuration

3. **High Cardinality**: Too many metric dimensions
   - Review tag values for high cardinality data
   - Consider sampling for high-volume scenarios

### Debugging

Enable verbose OpenTelemetry logging:

```json
{
  "Logging": {
    "LogLevel": {
      "OpenTelemetry": "Debug",
      "Azure.Monitor": "Debug"
    }
  }
}
```

## Performance Considerations

### Sampling

For high-traffic scenarios, configure sampling:

```csharp
builder.WithTracing(tracing =>
{
    tracing.SetSampler(new TraceIdRatioBasedSampler(0.1)); // 10% sampling
});
```

### Batch Export

Metrics and traces are batched for efficiency:
- Default batch size: 512 spans/metrics
- Default export interval: 5 seconds
- Configurable via environment variables

## Security

### Data Privacy

- No sensitive data in traces/metrics
- File contents not logged or traced
- Only metadata (size, type, duration) collected
- PII redacted in Application Insights

### Access Control

Application Insights access controlled via:
- Azure RBAC roles
- API keys for programmatic access
- Resource-level permissions

## Migration from Serilog

### What Changed

1. **Logging**: Now uses OpenTelemetry + Application Insights instead of Serilog
2. **Metrics**: Added comprehensive custom metrics
3. **Tracing**: Added distributed tracing capabilities
4. **Configuration**: New OpenTelemetry configuration section

### Backward Compatibility

- All log statements still work (use ILogger<T>)
- Same log levels and structure
- Console output still available for development
- Health checks and API functionality unchanged

## Best Practices

1. **Metric Naming**: Use descriptive, consistent names
2. **Tag Cardinality**: Limit tag values to prevent metric explosion  
3. **Sampling**: Use sampling for high-volume production environments
4. **Correlation**: Leverage trace correlation for debugging
5. **Alerts**: Set up proactive monitoring alerts
6. **Dashboards**: Create business-relevant dashboards
