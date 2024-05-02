namespace JSS.Test262Runner;

// FIXME: Change strings through-out code base to spans where applicable.
/// <summary>
/// Implements a trie based on splitting a path by its directories for storing test results.
/// </summary>
internal sealed class TestResultPathTrie
{
    /// <summary>
    /// Adds a test result to all the nodes that the path visits.
    /// </summary>
    /// <param name="path">The path to the test case file.</param>
    /// <param name="result">The result of the test.</param>
    public void Add(string path, TestResultType result)
    {
        _testResults.TryAdd(result, 0);
        _testResults[result]++;

        var nextPathSeperator = path.IndexOf(Path.DirectorySeparatorChar);
        if (nextPathSeperator == -1) return;

        var currentDirectory = path[..nextPathSeperator];
        if (!_leaves.ContainsKey(currentDirectory)) _leaves.Add(currentDirectory, new());

        var leaf = _leaves[currentDirectory];
        var nextPath = path[(nextPathSeperator + 1)..];
        leaf.Add(nextPath, result);
    }

    /// <summary>
    /// Visits every node in the trie using DFS and calls the <paramref name="visitor"/> callabck.
    /// </summary>
    /// <param name="visitor">A visitor that is called on each node of the trie.</param>
    /// <param name="path">The current path for the current node.</param>
    public void Visit(Action<string, Dictionary<TestResultType, int>> visitor, string path = "/")
    {
        if (path != "/") visitor(path, _testResults);

        foreach (var (nextDirectory, trieNode) in _leaves)
        {
            trieNode.Visit(visitor, path + $"/{nextDirectory}");
        }
    }

    static private Dictionary<TestResultType, int> CreateZeroedResultsDictionary()
    {
        return new()
        {
            { TestResultType.SUCCESS, 0 },
            { TestResultType.METADATA_PARSING_FAILURE, 0 },
            { TestResultType.HARNESS_EXECUTION_FAILURE, 0 },
            { TestResultType.PARSING_FAILURE, 0 },
            { TestResultType.CRASH_FAILURE, 0 },
            { TestResultType.FAILURE, 0 },
        };
    }

    private readonly Dictionary<TestResultType, int> _testResults = CreateZeroedResultsDictionary(); 
    private readonly Dictionary<string, TestResultPathTrie> _leaves = [];
}
