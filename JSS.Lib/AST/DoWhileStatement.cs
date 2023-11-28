using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.7.2 The do-while Statement, https://tc39.es/ecma262/#sec-do-while-statement
internal sealed class DoWhileStatement : INode
{
    public DoWhileStatement(IExpression whileExpression, INode iterationStatement)
    {
        WhileExpression = whileExpression;
        IterationStatement = iterationStatement;
    }

    // 14.7.2.2 Runtime Semantics: DoWhileLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-dowhileloopevaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let V be undefined.
        var V = vm.Undefined;

        // 2. Repeat,
        while (true)
        {
            // a. Let stmtResult be Completion(Evaluation of Statement).
            var stmtResult = IterationStatement.Evaluate(vm);

            // b. If LoopContinues(stmtResult, labelSet) is false, return ? UpdateEmpty(stmtResult, V).
            if (!stmtResult.LoopContinues().Value)
            {
                stmtResult.UpdateEmpty(V);
                return stmtResult;
            }

            // c. If stmtResult.[[Value]] is not EMPTY, set V to stmtResult.[[Value]].
            if (!stmtResult.IsValueEmpty())
            {
                V = stmtResult.Value;
            }

            // d. Let exprRef be ? Evaluation of Expression.
            var exprRef = WhileExpression.Evaluate(vm);
            if (exprRef.IsAbruptCompletion()) return exprRef;

            // e. Let exprValue be ? GetValue(exprRef).
            var exprValue = exprRef.Value.GetValue();
            if (exprValue.IsAbruptCompletion()) return exprValue;
            
            // f. If ToBoolean(exprValue) is false, return V.
            if (!exprValue.Value.ToBoolean().Value)
            {
                return Completion.NormalCompletion(V);
            }
        }
    }

    public IExpression WhileExpression { get; }
    public INode IterationStatement { get; }
}
