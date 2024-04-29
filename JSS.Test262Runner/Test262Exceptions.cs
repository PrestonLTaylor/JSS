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
    protected Test262Exception(string message) : base(message) { }

    protected Test262Exception(VM vm, Completion abruptCompletion) : base(Print.CompletionToString(vm, abruptCompletion))
    {
        Debug.Assert(abruptCompletion.IsAbruptCompletion());
    }
}

/// <summary>
/// Indicated that parsing/execution of a required harness file failed.
/// </summary>
internal class HarnessExecutionFailureException : Test262Exception
{
    public HarnessExecutionFailureException(string message) : base(message) { }
    public HarnessExecutionFailureException(VM vm, Completion completion) : base(vm, completion) { }
}
