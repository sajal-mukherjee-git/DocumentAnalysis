using DocumentUploadAPI.Endpoints;
using DocumentUploadAPI.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddDocumentTelemetry(builder.Configuration);
builder.Services.AddDocumentUploadServices(builder.Configuration);
builder.Services.AddApiDocumentation(builder.Configuration);

// Configure CORS for potential frontend integration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Document Upload API";
        options.Theme = ScalarTheme.Alternate;
    });
}

// Use HTTPS redirection in production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors();

// Map endpoints
app.MapDocumentUploadEndpoints();
app.MapHealthChecks("/health");

// Add a simple root endpoint
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
   .WithName("Root")
   .ExcludeFromDescription();

// Graceful shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("Application is shutting down...");
});

try
{
    app.Logger.LogInformation("Starting Document Upload API...");
    app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application terminated unexpectedly");
    throw;
}
