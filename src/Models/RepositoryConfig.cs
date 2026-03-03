namespace RawGitLab.Models;

public class RepositoryConfig
{
    public int ProjectId { get; set; }
    public string DefaultRef { get; set; } = "main";
}
