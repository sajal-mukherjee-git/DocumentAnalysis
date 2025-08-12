using DocumentUploadAPI.Models;
using DocumentUploadAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentUploadAPI.Endpoints;

public static class DocumentUploadEndpoints
{
    public static void MapDocumentUploadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/documents")
            .WithTags("Document Upload")
            .WithOpenApi();

        group.MapPost("/upload", UploadDocument)
            .WithName("UploadDocument")
            .WithSummary("Upload a document")
            .WithDescription("Uploads a document file to the configured storage location")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<UploadResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status500InternalServerError)
            .DisableAntiforgery(); // Required for file uploads

        group.MapGet("/health", HealthCheck)
            .WithName("HealthCheck")
            .WithSummary("Health check endpoint")
            .WithDescription("Returns the health status of the document upload service")
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] string? description,
        [FromServices] IDocumentUploadService uploadService,
        [FromServices] ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null)
            {
                logger.LogWarning("Upload request received with null file");
                var error = new ApiError("File is required", null, StatusCodes.Status400BadRequest);
                return Results.BadRequest(error);
            }

            logger.LogInformation("Received upload request for file: {FileName}", file.FileName);

            var request = new UploadRequest(file, description);
            var response = await uploadService.UploadDocumentAsync(request, cancellationToken);

            logger.LogInformation("Upload completed successfully for file ID: {FileId}", response.Id);

            return Results.Created($"/api/documents/{response.Id}", response);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation error during upload: {Message}", ex.Message);
            var error = new ApiError(ex.Message, null, StatusCodes.Status400BadRequest);
            return Results.BadRequest(error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during document upload");
            var error = new ApiError(
                "An error occurred while processing the upload.", 
                ex.Message, 
                StatusCodes.Status500InternalServerError);
            return Results.Problem(
                detail: error.Details,
                title: error.Message,
                statusCode: error.StatusCode);
        }
    }

    private static IResult HealthCheck(
        [FromServices] ILogger<Program> logger)
    {
        logger.LogDebug("Health check requested");
        
        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Document Upload API",
            Version = "1.0.0"
        });
    }
}
