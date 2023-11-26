using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.8 The continue Statement, https://tc39.es/ecma262/#prod-ContinueStatement
internal sealed class ContinueStatement : INode
{
    public ContinueStatement(Identifier? label)
    {
        Label = label;
    }

    // 14.8.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-continue-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let label be the StringValue of LabelIdentifier.
        var label = Label is not null ? Label.Name : "";

        // 2. Return Completion Record { [[Type]]: CONTINUE, [[Value]]: EMPTY, [[Target]]: label }.
        return Completion.ContinueCompletion(vm.Empty, label);
    }

    public Identifier? Label { get; }
}
