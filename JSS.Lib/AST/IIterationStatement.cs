using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal abstract class IIterationStatement : INode
{
    // 14.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-statement-semantics-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let newLabelSet be a new empty List.
        // 2. Return ? LabelledEvaluation of this BreakableStatement with argument newLabelSet.
        return LabelledEvaluation(vm, new());
    }

    // 14.13.4 Runtime Semantics: LabelledEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-labelledevaluation
    public Completion LabelledEvaluation(VM vm, List<string> labelSet)
    {
        // 1. Let stmtResult be Completion(LoopEvaluation of IterationStatement with argument labelSet).
        var stmtResult = LoopEvaluation(vm, labelSet);

        // 2. If stmtResult is a break completion, then
        if (stmtResult.IsBreakCompletion())
        {
            // a. If stmtResult.[[Target]] is EMPTY, then
            if (stmtResult.Target.Length == 0)
            {
                // i. If stmtResult.[[Value]] is EMPTY, set stmtResult to NormalCompletion(undefined).
                if (stmtResult.IsValueEmpty()) stmtResult = Undefined.The;
                // ii. Else, set stmtResult to NormalCompletion(stmtResult.[[Value]]).
                else stmtResult = stmtResult.Value;
            }
        }

        // 3. Return ? stmtResult.
        return stmtResult;
    }

    abstract public Completion LoopEvaluation(VM vm, List<string> labelSet);
}
