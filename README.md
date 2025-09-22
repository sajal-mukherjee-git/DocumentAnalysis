# Document Upload API
This is an ongoing  implementation of Document Analysis Project
## Features

- **Document Upload**: Single POST endpoint for uploading documents
- **File Validation**: Comprehensive validation for file size, type, and content
- **Configurable Storage**: Local file system storage with configurable paths
- **Layered Architecture**: Clean separation of concerns with service layers
- **OpenTelemetry Observability**: Comprehensive tracing, metrics, and logging
- **Application Insights**: Azure Monitor integration for production environments
- **Health Checks**: Built-in health monitoring
- **API Documentation**: Modern Scalar API documentation interface
- **Docker Ready**: Containerized with Docker and docker-compose support

## Project Structure

```
├── DocumentUploadAPI/          # Main API project
│   ├── Models/                 # Data models and DTOs
│   ├── Services/               # Business logic layer
│   ├── Endpoints/              # API endpoint definitions
│   ├── Extensions/             # Service registration extensions
│   ├── Program.cs              # Application entry point
│   ├── Dockerfile              # Container configuration
│   └── appsettings.json        # Configuration files
├── deployment/                 # Deployment configurations
│   ├── docker-compose.yml      # Main Docker Compose configuration
│   ├── docker-compose.prod.yml # Production overrides
│   ├── .env.example           # Environment variables template
│   ├── .vscode/               # VS Code debug configurations
│   └── scripts/               # Deployment scripts
└── README.md                  # This file
```

### Supported File Types
- PDF (.pdf)
- Word Documents (.docx, .doc)
- Text Files (.txt)
- Images (.png, .jpg, .jpeg)

### Configuration
- **Max File Size**: 100MB (configurable)
- **Storage Path**: Configurable via appsettings.json
- **Allowed Extensions**: Configurable list

## Quick Start

### Development with VS Code

1. **Open the deployment folder in VS Code**:
   ```bash
   cd deployment
   code .
   ```

2. **Press F5 to debug** - This will automatically start the application using Docker Compose

### Development Deployment (Command Line)

```bash
# Navigate to deployment directory
cd deployment

# Start the application
docker-compose up --build

# Access the application
# - API Documentation: http://localhost:8080
# - Health Check: http://localhost:8080/health
```

### Production Deployment

```bash
# Use production configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

### Local Development (without Docker)

1. **Navigate to the API project**:
   ```bash
   cd DocumentUploadAPI
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access the API**:
   - API Documentation: http://localhost:5000 (redirects to Scalar)
   - Scalar Documentation: http://localhost:5000/scalar/v1
   - Health Check: http://localhost:5000/health

## API Usage

### Upload Document

**Endpoint**: `POST /api/documents/upload`

**Content-Type**: `multipart/form-data`

**Parameters**:
- `file` (required): The file to upload
- `description` (optional): Description of the file

**Example using curl**:
```bash
curl -X POST http://localhost:8080/api/documents/upload \
  -F "file=@/path/to/your/document.pdf" \
  -F "description=Sample document upload"
```

**Example using PowerShell**:
```powershell
$headers = @{}
$form = @{
    file = Get-Item "C:\path\to\your\document.pdf"
    description = "Sample document upload"
}
Invoke-RestMethod -Uri "http://localhost:8080/api/documents/upload" -Method Post -Form $form
```

**Response** (201 Created):
```json
{
  "id": "abc123def456",
  "fileName": "abc123def456.pdf",
  "originalFileName": "document.pdf",
  "fileSize": 1024000,
  "contentType": "application/pdf",
  "uploadedAt": "2025-08-01T10:30:00Z",
  "storagePath": "/app/uploads/abc123def456.pdf"
}
```

**Error Response** (400 Bad Request):
```json
{
  "message": "File validation failed: File size exceeds maximum allowed size of 100 MB.",
  "statusCode": 400
}
```

## Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

### appsettings.json
```json
{
  "FileUpload": {
    "StoragePath": "/app/uploads",
    "MaxFileSizeBytes": 104857600,
    "AllowedExtensions": [".pdf", ".docx", ".doc", ".txt", ".png", ".jpg", ".jpeg"],
    "TempPath": "/app/temp"
  },
  "ApiSettings": {
    "Title": "Document Upload API",
    "Version": "v1",
    "Description": "A minimal API for document upload operations"
  }
}
```

### Environment Variable Overrides
You can override configuration using environment variables:
- `FileUpload__StoragePath`
- `FileUpload__MaxFileSizeBytes`
- `FileUpload__AllowedExtensions__0`, `FileUpload__AllowedExtensions__1`, etc.

### Docker Volumes

The following directories are mounted from the host:
- `./uploads` → `/app/uploads` (file storage)
- `./logs` → `/app/logs` (application logs)

### Ports

- **8080**: Application HTTP port
- **8443**: Application HTTPS port (production)
- **5000**: Local development HTTP port

## Health Checks

The application includes built-in health checks:
- **Endpoint**: `/health`
- **Interval**: 30 seconds
- **Timeout**: 10 seconds
- **Retries**: 3
- **Start Period**: 40 seconds

## Monitoring

### OpenTelemetry & Application Insights

- **OpenTelemetry**: Comprehensive observability with tracing, metrics, and logging
- **Application Insights**: Azure Monitor integration for production
- **Custom Metrics**: Upload rates, duration, file sizes, error rates
- **Distributed Tracing**: Request flow tracking across services

### Key Metrics Monitored
- Upload success/failure rates
- Upload duration and file sizes
- API response times
- System performance metrics

### Logs

View application logs:
```bash
docker-compose logs -f document-upload-api
```

### Status

Check container status:
```bash
docker-compose ps
```

## Scaling

Scale the application:
```bash
docker-compose up --scale document-upload-api=3
```

## Security Considerations

- File type validation based on extensions
- File size limitations
- Safe file naming (removes invalid characters)
- Runs as non-root user in Docker
- No execution of uploaded files

## Production Deployment

### Configuration Changes for Production

1. **Enable Application Insights**:
   ```json
   {
     "ApplicationInsights": {
       "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
     }
   }
   ```

2. **Configure HTTPS**:
   ```json
   {
     "Kestrel": {
       "Endpoints": {
         "Https": {
           "Url": "https://+:443"
         }
       }
     }
   }
   ```

3. **OpenTelemetry Settings**:
   ```json
   {
     "OpenTelemetry": {
       "ServiceName": "DocumentUploadAPI",
       "ServiceVersion": "1.0.0",
       "EnableConsoleExporter": false
     }
   }
   ```

### Docker in Production
```bash
# Build for production
docker build -t document-upload-api:prod ./DocumentUploadAPI

# Run with production settings
docker run -d \
  -p 443:8080 \
  -v /host/uploads:/app/uploads \
  -v /host/logs:/app/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ApplicationInsights__ConnectionString=your-connection-string" \
  --name document-upload-api-prod \
  document-upload-api:prod
```

## Development

### VS Code Integration

The project includes complete VS Code integration:
- **F5 Debugging**: Automatically starts Docker Compose and attaches debugger
- **Tasks**: Build, run, and debug tasks configured
- **Extensions**: Recommended extensions for .NET and Docker development

### Project Architecture
- **Models**: Data transfer objects and configuration models
- **Services**: Business logic with dependency injection
- **Endpoints**: Minimal API endpoint definitions
- **Extensions**: Service registration and configuration extensions

### Adding New Features
1. Define models in `Models/`
2. Implement business logic in `Services/`
3. Create endpoints in `Endpoints/`
4. Register services in `Extensions/ServiceCollectionExtensions.cs`

## Troubleshooting

### Common Issues

1. **Permission Denied**: Ensure upload directories exist and are writable
2. **File Size Errors**: Check `MaxFileSizeBytes` configuration
3. **Type Not Allowed**: Verify file extension is in `AllowedExtensions` list
4. **Port Conflicts**: Change port mapping in docker-compose.yml

### Logs Location
- **Container**: `/app/logs/`
- **Host** (with volume mount): `./logs/`

## Cleanup

Stop and remove containers:
```bash
docker-compose down
```

Remove volumes (⚠️ **Warning**: This will delete uploaded files):
```bash
docker-compose down -v
```

## Prerequisites

- .NET 10 SDK (for local development)
- Docker (for containerized deployment)
- VS Code (recommended for development)

## License

This project is provided as-is for educational and development purposes.
