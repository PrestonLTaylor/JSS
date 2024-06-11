using JSS.Lib.Execution;
using JSS.Lib.AST.Values;

namespace JSS.Lib.AST;

// 14.4 Empty Statement, https://tc39.es/ecma262/#sec-empty-statement
internal sealed class EmptyStatement : INode
{
    // 14.4.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-empty-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return EMPTY.
        return Empty.The;
    }
}
