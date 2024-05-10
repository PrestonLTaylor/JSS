using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// FIXME: Parse and evaluate LabelledStatements
internal class LabelledStatement
{
    // 14.13.4 Runtime Semantics: LabelledEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-labelledevaluation
    static public Completion LabelledBreakableEvaluation(VM vm, IBreakableStatement breakableStatement)
    {
        // 1. Let stmtResult be Completion(LoopEvaluation of IterationStatement FIXME: with argument labelSet)/Completion(Evaluation of SwitchStatement).
        var stmtResult = breakableStatement.EvaluateFromLabelled(vm);

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
}
