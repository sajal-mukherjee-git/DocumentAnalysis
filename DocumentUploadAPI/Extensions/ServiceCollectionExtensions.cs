using DocumentUploadAPI.Models;
using DocumentUploadAPI.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace DocumentUploadAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentUploadServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<FileUploadOptions>(configuration.GetSection(FileUploadOptions.SectionName));
        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));
        services.Configure<OpenTelemetryOptions>(configuration.GetSection(OpenTelemetryOptions.SectionName));

        // Register FileUploadOptions as singleton for injection
        services.AddSingleton<FileUploadOptions>(provider => 
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return config.GetSection(FileUploadOptions.SectionName).Get<FileUploadOptions>() ?? new FileUploadOptions();
        });

        // Register services
        services.AddScoped<IFileValidationService, FileValidationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDocumentUploadService, DocumentUploadService>();

        return services;
    }

    public static IServiceCollection AddDocumentTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ApplicationInsights") ?? configuration["ApplicationInsights:ConnectionString"];

        // Add Application Insights if connection string is provided
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = connectionString;
            });
        }

        // Add custom metrics
        services.AddSingleton<DocumentUploadMetrics>();

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>() ?? new ApiSettings();

        // Add OpenAPI support (built into .NET 10)
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        
        return services;
    }
}
