namespace RawGitLab.Services;

public interface IGitLabService
{
    Task<HttpContent> GetFileContentAsync(
        int projectId, 
        string refName, 
        string filePath, 
        CancellationToken cancellationToken = default);
    
    Task<int> GetProjectIdByPathAsync(string groupProject, CancellationToken cancellationToken = default);
}
