using System.Diagnostics;

namespace JSS.Test262Runner;

/// <summary>
/// Clones a target GitHub repository, if the repository is not already present.
/// </summary>
internal sealed class GitHubCloner
{
    /// <summary>
    /// Constructs a <see cref="GitHubCloner"/> that can clone a specified GitHub repository, if the repository is not already present.
    /// </summary>
    /// <param name="targetRepository">The repository to clone with the format "GITHUB_USERNAME/REPOSITORY_NAME".</param>
    public GitHubCloner(string targetRepository)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetRepository, nameof(targetRepository));
        if (!targetRepository.Contains('/'))
        {
            throw new ArgumentException($"'{targetRepository}' is not a valid target GitHub repository.", nameof(targetRepository));
        }

        _repositoryName = targetRepository.Split('/')[1];
        _repositoryGitUri = new($"https://www.github.com/{targetRepository}.git");

    }

    /// <summary>
    /// Checks if a the target repository already exists in the current directory.<br/>
    /// If it does not exist, the repository will be cloned to the current directory.
    /// </summary>
    public void CloneRepositoryIfNotAlreadyPresent()
    {
        if (Directory.Exists($"./{_repositoryName}"))
        {
            Console.WriteLine($"The {_repositoryName} git repository already exists...");
            return;
        }

        Console.WriteLine($"The {_repositoryName} git repository does not exist, starting git clone...");

        CloneRepository();

        Console.WriteLine($"Finished cloning the {_repositoryName} git repository  repository.");
    }

    /// <summary>
    /// Clones the specified repository to the current directory.
    /// </summary>
    private void CloneRepository()
    {
        var cloneProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C git clone --depth 1 {_repositoryGitUri}"
            }
        };

        cloneProcess.Start();
        cloneProcess.WaitForExit();
    }

    private readonly string _repositoryName;
    private readonly Uri _repositoryGitUri;
}
