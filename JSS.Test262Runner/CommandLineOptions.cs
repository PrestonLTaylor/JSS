using CommandLine;

namespace JSS.Test262Runner;

internal sealed class CommandLineOptions
{
    [Option('q', "quiet", FlagCounter = true, HelpText = "Enables quiet mode. Won't output results for every test executed.")]
    public int QuietCount { get; set; }
    public bool Quiet => QuietCount > 0;
}
