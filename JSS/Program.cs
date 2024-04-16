using JSS.Lib;
using JSS.CLI;
using JSS.Lib.Execution;

var completion = Realm.InitializeHostDefinedRealm(out VM globalVm);
if (completion.IsAbruptCompletion())
{
    Print.PrintCompletion(completion);
    return;
}

while (true)
{
    Console.Write("> ");

    try
    {
        var input = Console.ReadLine() ?? "";
        var parser = new Parser(input);
        var script = parser.Parse(globalVm);
        var result = script.ScriptEvaluation();
        Print.PrintCompletion(result);
    }
    catch (Exception e)
    {
        Print.PrintException(e);
    }

    // NOTE: We need this empty write line to prevent the background color from spilling over
    Console.WriteLine();
}