using JSS.Lib;
using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Common;

public sealed class Print
{
    static public void PrintCompletion(VM vm, Completion completion)
    {
        var completionAsString = CompletionToString(vm, completion);
        if (completion.IsAbruptCompletion())
        {
            SetToErrorColors();
        }
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write(completionAsString);

        Console.ResetColor();
    }

    static public string CompletionToString(VM vm, Completion completion)
    {
        if (completion.IsThrowCompletion())
        {
            return $"Uncaught {ValueToString(vm, completion.Value)}";
        }
        else
        {
            return ValueToString(vm, completion.Value);
        }
    }

    static private string ValueToString(VM vm, Value value)
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
        else if (value.IsObject())
        {
            var asObject = value.AsObject();
            return asObject.ToString(vm);
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
