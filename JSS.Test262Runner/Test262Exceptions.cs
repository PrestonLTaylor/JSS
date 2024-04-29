using JSS.Common;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Test262Runner;

/// <summary>
/// An <see cref="Exception"/> thrown during execution of a Test262 test case. <br/>
/// Intended to be inherited from to represent a specific failure of a test case.
/// </summary>
internal class Test262Exception : Exception
{
    protected Test262Exception(VM vm, Completion abruptCompletion) : base(CreateTest262ExceptionMessage(vm, abruptCompletion))
    {
        Debug.Assert(abruptCompletion.IsAbruptCompletion());
    }

    static private string CreateTest262ExceptionMessage(VM vm, Completion abruptCompletion)
    {
        var completionType = CompletionTypeToString(abruptCompletion);
        var completionValue = Print.CompletionToString(vm, abruptCompletion);

        return $"Completion Type: {completionType}\nCompletion Value:{completionValue}";
    }

    static private string CompletionTypeToString(Completion completion)
    {
        if (completion.IsThrowCompletion()) return "Throw Completion";
        if (completion.IsReturnCompletion()) return "Return Completion";
        if (completion.IsBreakCompletion()) return "Break Completion";
        if (completion.IsContinueCompletion()) return "Continue Completion";
        return "Normal Completion";
    }
}

