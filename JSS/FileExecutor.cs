using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.CLI;

internal sealed class FileExecutor
{
    public FileExecutor(VM globalVm, string filePath)
    {
        _vm = globalVm;
        _filePath = filePath;
    }

    public void ExecuteFile()
    {
        try
        {
            var fileContent = File.ReadAllText(_filePath);
            var parser = new Parser(fileContent);
            var script = parser.Parse(_vm);
            var result = script.ScriptEvaluation();
            Print.PrintCompletion(_vm, result);
        }
        catch (Exception e)
        {
            Print.PrintException(e);
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private readonly VM _vm;
    private readonly string _filePath;
}
