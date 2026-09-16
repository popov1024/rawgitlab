namespace RawGitLab.Models;

public class GitLabSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PrivateToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Disables SSL certificate validation for self-signed certificates.
    /// Use only for trusted self-hosted GitLab instances.
    /// </summary>
    public bool SkipCertificateValidation { get; set; }
}
