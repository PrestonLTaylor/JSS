using CommandLine;

namespace JSS.Test262Runner;

internal sealed class CommandLineOptions
{
    [Option('q', "quiet", FlagCounter = true, HelpText = "Enables quiet mode. Won't output results for every test executed.")]
    public int QuietCount { get; set; }
    public bool Quiet => QuietCount > 0;

    [Option('f', "filter", Default = "*.js",
        HelpText = "Filters tests to execute based on their path. * is a wildcard for zero or more characters and ? is a wildcard for zero or one characters.")]
    public required string Filter { get; set; }
}
