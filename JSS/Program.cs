using CommandLine;
using JSS.CLI;
using JSS.Lib.Execution;

var completion = Realm.InitializeHostDefinedRealm(out VM globalVm);
if (completion.IsAbruptCompletion())
{
    Print.PrintCompletion(globalVm, completion);
    return;
}

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(options =>
    {
        if (!string.IsNullOrEmpty(options.Script))
        {
            var fileExecutor = new FileExecutor(globalVm, options.Script);
            fileExecutor.ExecuteFile();
        }
        else
        {
            var repl = new Repl(globalVm);
            repl.ExecuteRepl();
        }
    });
