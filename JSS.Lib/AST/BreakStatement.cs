using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Reflection.Emit;

namespace JSS.Lib.AST;

// 14.9 The break Statement, https://tc39.es/ecma262/#sec-break-statement
internal sealed class BreakStatement : INode
{
    public BreakStatement(Identifier? label)
    {
        Label = label;
    }

    // 14.9.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-break-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let label be the StringValue of LabelIdentifier.
        var label = Label is not null ? Label.Name : "";

        // 2. Return Completion Record { [[Type]]: BREAK, [[Value]]: EMPTY, [[Target]]: label }.
        return Completion.BreakCompletion(Empty.The, label);
    }

    public Identifier? Label { get; }
}
