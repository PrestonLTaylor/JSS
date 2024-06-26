﻿using JSS.Lib.Execution;
using JSS.Lib.AST.Values;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 14.16 The debugger Statement, https://tc39.es/ecma262/#sec-debugger-statement
internal sealed class DebuggerStatement : INode
{
    // 14.16.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-debugger-statement-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. If an implementation-defined debugging facility is available and enabled, then
        if (Debugger.IsAttached)
        {
            // a. Perform an implementation-defined debugging action.
            Debugger.Break();

            // b. Return a new implementation-defined Completion Record.
            return Empty.The;
        }

        // 2. Else,
        // a. Return EMPTY.
        return Empty.The;
    }
}