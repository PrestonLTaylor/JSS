namespace JSS.Test262Runner;

// FIXME: Accept a filter from the command line to filter the tests being executed
/// <summary>
/// Our implementation for a runner for the test-262 test suite using the "test262" git repository that should be in the same directory.
/// </summary>
internal sealed class Test262Runner
{
    public Test262Runner()
    {
        _harnessNameToContent = ReadHarnessFiles();
    }

    /// <summary>
    /// Reads the harness files required for running tests from the test-262 repository.
    /// </summary>
    /// <returns>A map of the harness file name to the harness' file content.</returns>
    static private Dictionary<string, string> ReadHarnessFiles()
    {
        const string HARNESS_DIRECTORY = "./test262/harness";
        const string HARNESS_JAVASCRIPT_FILTER = "*.js";

        var harnessNameToContent = new Dictionary<string, string>();
        var harnessFiles = Directory.EnumerateFiles(HARNESS_DIRECTORY, HARNESS_JAVASCRIPT_FILTER, SearchOption.AllDirectories);
        foreach (var harnessFile in harnessFiles)
        {
            var fileName = Path.GetFileName(harnessFile);
            var fileContent = File.ReadAllText(harnessFile);
            harnessNameToContent.Add(fileName, fileContent);
        }

        return harnessNameToContent;
    }

    private readonly Dictionary<string, string> _harnessNameToContent;
}
