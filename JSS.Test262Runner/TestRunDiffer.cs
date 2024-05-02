using System.Text.Json;

namespace JSS.Test262Runner;

/// <summary>
/// Performs and logs the difference between two test result output files
/// </summary>
internal sealed class TestRunDiffer
{
    public TestRunDiffer(DiffOptions diffOptions) 
    {
        _fromTestRun = File.ReadAllText(diffOptions.From);
        _toTestRun = File.ReadAllText(diffOptions.To);
    }

    /// <summary>
    /// Logs the difference between provided tests and outputs them to the console.
    /// </summary>
    public void LogTestsDifferences()
    {
        var fromResults = JsonSerializer.Deserialize<Dictionary<string, TestResult>>(_fromTestRun);
        if (fromResults is null)
        {
            Console.WriteLine("The 'from' test run file is not valid.");
            return;
        }

        var toResults = JsonSerializer.Deserialize<Dictionary<string, TestResult>>(_toTestRun);
        if (toResults is null)
        {
            Console.WriteLine("The 'to' test run file is not valid.");
            return;
        }

        var diffKeys = GetKeysOfTestsWithDifferences(fromResults, toResults);
        if (diffKeys.Count == 0)
        {
            Console.WriteLine("No differences between the two test runs found.");
            return;
        }

        foreach (var diffKey in diffKeys)
        {
            var fromResult = fromResults[diffKey];
            var toResult = toResults[diffKey];

            if (toResult.Type == TestResultType.SUCCESS) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"{fromResult.TestPath}: {TestResult.TEST_RESULT_TYPE_TO_EMOJI[fromResult.Type]} => {TestResult.TEST_RESULT_TYPE_TO_EMOJI[toResult.Type]}");
            if (toResult.FailureReason != "") Console.WriteLine(toResult.FailureReason);

            Console.ResetColor();
        }
    }

    /// <summary>
    /// Gets the keys from the "from" and "to" results that have test result differences.
    /// </summary>
    /// <returns>The keys of from and to that have different test results.</returns>
    static private IReadOnlyList<string> GetKeysOfTestsWithDifferences(Dictionary<string, TestResult> from, Dictionary<string, TestResult> to)
    {
        return from
            .Where(x =>
            {
                if (!to.TryGetValue(x.Key, out var toResult)) return false;

                var fromResult = from[x.Key];
                return !fromResult.Equals(toResult);
            })
            .Select(kv => kv.Key)
            .ToList();
    }

    private readonly string _fromTestRun;
    private readonly string _toTestRun;
}
