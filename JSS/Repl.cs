using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.CLI;

internal sealed class Repl
{
    public Repl(VM globalVm)
    {
        _vm = globalVm;
    }

    public void ExecuteRepl()
    {
        while (true)
        {
            Console.Write("> ");
            EvaluateLine();
        }
    }

    private void EvaluateLine()
    {
        try
        {
            var input = Console.ReadLine() ?? "";
            var parser = new Parser(input);
            var script = parser.Parse(_vm);
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

    private readonly VM _vm;
}
