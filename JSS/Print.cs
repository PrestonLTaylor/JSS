using JSS.Lib;
using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;
using String = JSS.Lib.AST.Values.String;

namespace JSS.CLI;

internal sealed class Print
{
    static public string PrintCompletion(Completion completion)
    {
        if (completion.IsThrowCompletion())
        {
            // FIXME: Print the exception type
            return $"Uncaught {PrintValue(completion.Value)}";
        }
        else
        {
            return PrintValue(completion.Value);
        }
    }

    static public string PrintValue(Value value)
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
            var asBoolean = (value as Boolean)!;
            return asBoolean.Value ? "true" : "false";
        }
        else if (value.IsString())
        {
            var asString = (value as String)!;
            return $"\"{asString.Value}\"";
        }
        else if (value.IsNumber())
        {
            var asNumber = (value as Number)!;
            return asNumber.Value.ToString();
        }

        return value.Type().ToString();
    }

    static public string PrintException(Exception e)
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
}
