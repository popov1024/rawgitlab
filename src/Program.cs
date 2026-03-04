using RawGitLab.Models;
using RawGitLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration - AOT-compatible binding
var gitLabSection = builder.Configuration.GetSection("GitLab");
var gitLabSettings = new GitLabSettings
{
    BaseUrl = gitLabSection["BaseUrl"] ?? string.Empty,
    PrivateToken = gitLabSection["PrivateToken"] ?? string.Empty
};
builder.Services.AddSingleton(gitLabSettings);

// Services - AOT-compatible (no IHttpClientFactory)
builder.Services.AddSingleton<IGitLabService>(sp =>
{
    var httpClient = new HttpClient();
    var settings = sp.GetRequiredService<GitLabSettings>();
    var logger = sp.GetRequiredService<ILogger<GitLabService>>();
    return new GitLabService(httpClient, settings, logger);
});

var app = builder.Build();

// Endpoints
// Supports format: /group/project/-/raw/ref/file_path
// Example: /arch/components/-/raw/main/person-customer.iuml
app.MapGet("/{*path}", async (
    string? path,
    IGitLabService gitLabService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrEmpty(path))
    {
        return Results.Ok(new { Status = "OK", GitLabUrl = gitLabSettings.BaseUrl });
    }

    var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Expected: group/project/-/raw/ref/file_path
    // Find "-/raw" pattern in the path
    var rawIndex = Array.IndexOf(parts, "raw");
    if (rawIndex < 1 || parts[rawIndex - 1] != "-")
    {
        return Results.BadRequest("Invalid path format. Expected: group/project/-/raw/ref/file_path");
    }

    // Minimum: group/project/-/raw/ref (5 parts)
    if (rawIndex < 3)
    {
        return Results.BadRequest("Invalid path format. Expected: group/project/-/raw/ref/file_path");
    }

    // group/project can contain multiple parts (e.g., group/subgroup/project)
    var groupProject = string.Join("/", parts.Take(rawIndex - 1));
    var refName = parts[rawIndex + 1];
    var filePath = string.Join("/", parts.Skip(rawIndex + 2));

    if (string.IsNullOrEmpty(filePath))
    {
        return Results.BadRequest("File path not specified");
    }

    try
    {
        // Get project_id by group/project path
        var projectId = await gitLabService.GetProjectIdByPathAsync(groupProject, cancellationToken);

        var fileContent = await gitLabService.GetFileContentAsync(
            projectId,
            refName,
            filePath,
            cancellationToken);

        var contentType = fileContent.Headers.ContentType?.ToString() ?? "application/octet-stream";
        var fileName = Path.GetFileName(filePath);

        return Results.Stream(
            await fileContent.ReadAsStreamAsync(cancellationToken),
            contentType,
            fileName,
            enableRangeProcessing: true);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(ex.Message, statusCode: 404);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
});

app.Run();
