using JSS.Lib;
using JSS.CLI;

// FIXME: We currently use "InternalsVisibleTo" for JSS.Lib, we should fix that
while (true)
{
    Console.Write("> ");

    try
    {
        var input = Console.ReadLine() ?? "";
        var parser = new Parser(input);
        var script = parser.Parse();
        var result = script.ScriptEvaluation();

        if (result.IsThrowCompletion())
        {
            Console.BackgroundColor = ConsoleColor.Red;
        }
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(Print.PrintCompletion(result));
    }
    catch (Exception e)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(Print.PrintException(e));
    }

    Console.ResetColor();

    // NOTE: We need this empty write line to prevent the background color from spilling over
    Console.WriteLine();
}