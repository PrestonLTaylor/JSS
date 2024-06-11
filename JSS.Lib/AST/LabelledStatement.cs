using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// FIXME: Parse and evaluate LabelledStatements
internal sealed class LabelledStatement : INode
{
    public LabelledStatement(Identifier identifier, INode labelledItem)
    {
        Identifier = identifier;
        LabelledItem = labelledItem;
    }


    // 14.13.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-labelled-statements-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? LabelledEvaluation of this LabelledStatement with argument « ».
        return LabelledEvaluation(vm, new());
    }

    // 14.13.4 Runtime Semantics: LabelledEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-labelledevaluation
    private Completion LabelledEvaluation(VM vm, List<string> labelSet)
    {
        // 1. Let label be the StringValue of LabelIdentifier.
        // 2. Let newLabelSet be the list-concatenation of labelSet and « label ».
        // NOTE: If labelSet ever needs to be used after Step 3, we need to create a new list.
        labelSet.Add(Identifier.Name);

        // 3. Let stmtResult be Completion(LabelledEvaluation of LabelledItem with argument newLabelSet).
        Completion stmtResult;
        if (LabelledItem is LabelledStatement innerLabelledStatement)
        {
            stmtResult = innerLabelledStatement.LabelledEvaluation(vm, labelSet);
        }
        else if (LabelledItem is SwitchStatement switchStatement)
        {
            stmtResult = switchStatement.Evaluate(vm);
        }
        else if (LabelledItem is IIterationStatement iterationStatement)
        {
            stmtResult = iterationStatement.LabelledEvaluation(vm, labelSet);
        }
        else
        {
            stmtResult = LabelledItem.Evaluate(vm);
        }

        // 4. If stmtResult is a break completion and stmtResult.[[Target]] is label, then
        if (stmtResult.IsBreakCompletion() && stmtResult.Target == Identifier.Name)
        {
            // a. Set stmtResult to NormalCompletion(stmtResult.[[Value]]).
            stmtResult = stmtResult.Value;
        }

        // 5. Return ? stmtResult.
        return stmtResult;
    }

    public Identifier Identifier { get; }
    public INode LabelledItem { get; }
}
