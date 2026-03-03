using System.Net.Http.Headers;
using System.Text.Json;
using RawGitLab.Models;

namespace RawGitLab.Services;

public class GitLabService : IGitLabService
{
    private readonly HttpClient _httpClient;
    private readonly GitLabSettings _settings;
    private readonly ILogger<GitLabService> _logger;

    public GitLabService(HttpClient httpClient, GitLabSettings settings, ILogger<GitLabService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        
        if (!string.IsNullOrEmpty(_settings.PrivateToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _settings.PrivateToken);
        }
    }

    public async Task<HttpContent> GetFileContentAsync(
        int projectId,
        string refName,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var encodedPath = Uri.EscapeDataString(filePath);
        var url = $"/api/v4/projects/{projectId}/repository/files/{encodedPath}/raw?ref={Uri.EscapeDataString(refName)}";

        _logger.LogDebug("GitLab request: {Url}", url);

        var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = response.StatusCode == System.Net.HttpStatusCode.NotFound
                ? $"File '{filePath}' not found in project {projectId} (ref: {refName})"
                : $"GitLab API error: {response.StatusCode}";

            _logger.LogWarning(error);
            throw new HttpRequestException(error);
        }

        return response.Content;
    }
    
    /// <summary>
    /// Get project_id by group/project path
    /// </summary>
    public async Task<int> GetProjectIdByPathAsync(string groupProject, CancellationToken cancellationToken = default)
    {
        var encodedPath = Uri.EscapeDataString(groupProject);
        var url = $"/api/v4/projects/{encodedPath}";
        
        _logger.LogDebug("GitLab request: {Url}", url);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Project '{groupProject}' not found: {response.StatusCode}");
        }
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        
        if (doc.RootElement.TryGetProperty("id", out var idElement))
        {
            return idElement.GetInt32();
        }
        
        throw new InvalidOperationException("Failed to get project_id from GitLab response");
    }
}
