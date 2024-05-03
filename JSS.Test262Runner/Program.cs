// TODO: Setup ILogger (or something similar) that we can use for this project
using CommandLine;
using JSS.Test262Runner;
using System.Text;


Console.OutputEncoding = Encoding.UTF8;

Parser.Default.ParseArguments<DiffOptions, RunnerOptions>(args)
    .WithParsed<DiffOptions>(options =>
    {
        Console.WriteLine("\nStarting the test-262 differ...");
        var differ = new TestRunDiffer(options);
        differ.LogTestsDifferences();
    })
    .WithParsed<RunnerOptions>(options =>
    {
        var test262RepositoryCloner = new GitHubCloner("tc39/test262");
        test262RepositoryCloner.CloneRepositoryIfNotAlreadyPresent();

        Console.WriteLine("\nStarting the test-262 runner...");
        var test262Runner = new Test262Runner(options);
        test262Runner.StartRunner();
    });
