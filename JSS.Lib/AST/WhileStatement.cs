using JSS.Lib.AST.Values;

using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.7.3 The while Statement, https://tc39.es/ecma262/#sec-while-statement
internal sealed class WhileStatement : INode
{
    public WhileStatement(IExpression whileExpression, INode iterationStatement)
    {
        WhileExpression = whileExpression;
        IterationStatement = iterationStatement;
    }

    // 14.7.3.2 Runtime Semantics: WhileLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-whileloopevaluation
    override public Completion Evaluate(VM vm)
    {
        // 1.Let V be undefined.
        var V = (Value)vm.Undefined;

        // 2.Repeat,
        while (true)
        {
            // a. Let exprRef be ? Evaluation of Expression.
            var exprRef = WhileExpression.Evaluate(vm);
            if (exprRef.IsAbruptCompletion()) return exprRef;

            // b. Let exprValue be ? GetValue(exprRef).
            var exprValue = exprRef.Value.GetValue();
            if (exprValue.IsAbruptCompletion()) return exprValue;

            // c. If ToBoolean(exprValue) is false, return V.
            if (!exprValue.Value.ToBoolean().Value)
            {
                return Completion.NormalCompletion(V);
            }

            // d. Let stmtResult be Completion(Evaluation of Statement).
            var stmtResult = IterationStatement.Evaluate(vm);

            // e. If LoopContinues(stmtResult, labelSet) is false, return ? UpdateEmpty(stmtResult, V).
            if (!stmtResult.LoopContinues().Value)
            {
                stmtResult.UpdateEmpty(V);
                return stmtResult;
            }

            // f. If stmtResult.[[Value]] is not EMPTY, set V to stmtResult.[[Value]].
            if (!stmtResult.IsValueEmpty())
            {
                V = stmtResult.Value;
            }
        }
    }

    public IExpression WhileExpression { get; }
    public INode IterationStatement { get; }
}
