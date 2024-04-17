using CommandLine;

namespace JSS.CLI;

internal interface IExecutionOptions
{
    [Option('s', "script", HelpText = "Path to a JavaScript file to execute.")]
    public string? Script { get; }
}

internal sealed class CommandLineOptions : IExecutionOptions
{
    public CommandLineOptions(string? script)
    {
        Script = script;
    }

    public string? Script { get; }
}
