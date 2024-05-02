using CommandLine;

namespace JSS.Test262Runner;

[Verb("diff", HelpText = "Reports the difference between two test runs of the runner.")]
internal sealed class DiffOptions
{
    [Option('f', "from", HelpText = "File path to the test run to diff from.", Required = true)]
    public required string From { get; set; }

    [Option('t', "to", HelpText = "File path to the test run to diff to.", Required = true)]
    public required string To { get; set; }
}

[Verb("runner", isDefault: true, HelpText = "Performs a test run of the test-262 test suite.")]
internal sealed class RunnerOptions
{
    [Option('q', "quiet", FlagCounter = true, HelpText = "Enables quiet mode. Won't output results for every test executed.")]
    public int QuietCount { get; set; }
    public bool Quiet => QuietCount > 0;

    [Option('f', "filter", Default = "*.js",
        HelpText = "Filters tests to execute based on their path. * is a wildcard for zero or more characters and ? is a wildcard for zero or one characters.")]
    public required string Filter { get; set; }
}
