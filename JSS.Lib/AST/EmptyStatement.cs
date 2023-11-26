﻿using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.4 Empty Statement, https://tc39.es/ecma262/#sec-empty-statement
internal sealed class EmptyStatement : INode
{
    // 14.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-empty-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return EMPTY.
        return Completion.NormalCompletion(vm.Empty);
    }
}
