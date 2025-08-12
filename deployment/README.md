# Document Upload API - Deployment

This directory contains deployment configurations for the Document Upload API.

## Structure

```
deployment/
├── docker-compose.yml          # Main Docker Compose configuration
├── docker-compose.prod.yml     # Production overrides
├── .env.example               # Environment variables template
├── nginx/                     # Nginx reverse proxy configuration
└── scripts/                   # Deployment scripts
```

## Quick Start

### Development Deployment

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

## Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

### Volumes

The following directories are mounted from the host:
- `./uploads` → `/app/uploads` (file storage)
- `./logs` → `/app/logs` (application logs)

### Ports

- **8080**: Application HTTP port
- **8443**: Application HTTPS port (production)

## Health Checks

The application includes built-in health checks:
- **Endpoint**: `/health`
- **Interval**: 30 seconds
- **Timeout**: 10 seconds
- **Retries**: 3
- **Start Period**: 40 seconds

## Monitoring

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

## Cleanup

Stop and remove containers:
```bash
docker-compose down
```

Remove volumes (⚠️ **Warning**: This will delete uploaded files):
```bash
docker-compose down -v
```
