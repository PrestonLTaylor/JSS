using JSS.Lib;
using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;
using String = JSS.Lib.AST.Values.String;

namespace JSS.CLI;

internal sealed class Print
{
    static public void PrintCompletion(Completion completion)
    {
        var completionAsString = CompletionToString(completion);
        if (completion.IsAbruptCompletion())
        {
            SetToErrorColors();
        }
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(completionAsString);

        Console.ResetColor();
    }

    static private string CompletionToString(Completion completion)
    {
        if (completion.IsThrowCompletion())
        {
            // FIXME: Print the exception type
            return $"Uncaught {ValueToString(completion.Value)}";
        }
        else
        {
            return ValueToString(completion.Value);
        }
    }

    static private string ValueToString(Value value)
    {
        if (value.IsEmpty())
        {
            return "EMPTY";
        }
        else if (value.IsUndefined())
        {
            return "undefined";
        }
        else if (value.IsNull())
        {
            return "null";
        }
        else if (value.IsBoolean())
        {
            var asBoolean = value.AsBoolean();
            return asBoolean.Value ? "true" : "false";
        }
        else if (value.IsString())
        {
            var asString = value.AsString();
            return $"\"{asString.Value}\"";
        }
        else if (value.IsNumber())
        {
            var asNumber = value.AsNumber();
            return asNumber.Value.ToString();
        }
        else if (value.HasInternalCall())
        {
            return "Function";
        }

        return value.Type().ToString();
    }

    static public void PrintException(Exception e)
    {
        SetToErrorColors();
        Console.Write(ExceptionToString(e));
        Console.ResetColor();
    }

    static private string ExceptionToString(Exception e)
    {
        if (e is SyntaxErrorException)
        {
            return $"Uncaught {e.Message}";
        }
        else
        {
            return $"Internal Error: {e.Message}";
        }
    }

    static private void SetToErrorColors()
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
    }
}
