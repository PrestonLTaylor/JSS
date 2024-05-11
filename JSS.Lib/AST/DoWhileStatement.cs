using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.7.2 The do-while Statement, https://tc39.es/ecma262/#sec-do-while-statement
internal sealed class DoWhileStatement : IIterationStatement
{
    public DoWhileStatement(IExpression whileExpression, INode iterationStatement)
    {
        WhileExpression = whileExpression;
        IterationStatement = iterationStatement;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Return the VarDeclaredNames of Statement.
        return IterationStatement.VarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Return the VarScopedDeclarations of Statement.
        return IterationStatement.VarScopedDeclarations();
    }

    // 14.7.2.2 Runtime Semantics: DoWhileLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-dowhileloopevaluation
    public override Completion LoopEvaluation(VM vm, List<string> labelSet)
    {
        // 1. Let V be undefined.
        var V = (Value)Undefined.The;

        // 2. Repeat,
        while (true)
        {
            // a. Let stmtResult be Completion(Evaluation of Statement).
            var stmtResult = IterationStatement.Evaluate(vm);

            // b. If LoopContinues(stmtResult, labelSet) is false, return ? UpdateEmpty(stmtResult, V).
            if (!stmtResult.LoopContinues(labelSet))
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
            var exprValue = exprRef.Value.GetValue(vm);
            if (exprValue.IsAbruptCompletion()) return exprValue;
            
            // f. If ToBoolean(exprValue) is false, return V.
            if (!exprValue.Value.ToBoolean().Value)
            {
                return V;
            }
        }
    }

    public IExpression WhileExpression { get; }
    public INode IterationStatement { get; }
}
