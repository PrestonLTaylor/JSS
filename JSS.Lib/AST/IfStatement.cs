using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
internal sealed class IfStatement : INode
{
    public IfStatement(IExpression ifExpression, INode ifCaseStatement, INode? elseCaseStatement)
    {
        IfExpression = ifExpression;
        IfCaseStatement = ifCaseStatement;
        ElseCaseStatement = elseCaseStatement;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Let names1 be VarDeclaredNames of the first Statement.
        var names = IfCaseStatement.VarDeclaredNames();

        // 2. Let names2 be VarDeclaredNames of the second Statement.
        if (ElseCaseStatement is not null)
        {
            names.AddRange(ElseCaseStatement.VarDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be VarScopedDeclarations of the first Statement.
        var declarations = IfCaseStatement.VarScopedDeclarations();

        // 2. Let declarations2 be VarScopedDeclarations of the second Statement.
        if (ElseCaseStatement is not null)
        {
            declarations.AddRange(ElseCaseStatement.VarScopedDeclarations());
        }

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 14.6.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-if-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        if (ElseCaseStatement is null)
        {
            return EvaluateWithoutElse(vm);
        }
        else
        {
            return EvaluateWithElse(vm);
        }
    }

    private Completion EvaluateWithoutElse(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = IfExpression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let exprValue be ToBoolean(? GetValue(exprRef)).
        var getResult = exprRef.Value.GetValue(vm);
        if (getResult.IsAbruptCompletion()) return getResult;

        var exprValue = getResult.Value.ToBoolean();

        // 3. If exprValue is false, then
        if (!exprValue.Value)
        {
            // a. Return undefined.
            return Undefined.The;
        }
        // 4. Else,
        else
        {
            // a. Let stmtCompletion be Completion(Evaluation of Statement).
            var stmtCompletion = IfCaseStatement.Evaluate(vm);

            // b. Return ? UpdateEmpty(stmtCompletion, undefined).
            stmtCompletion.UpdateEmpty(Undefined.The);
            return stmtCompletion;
        }
    }

    private Completion EvaluateWithElse(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = IfExpression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let exprValue be ToBoolean(? GetValue(exprRef)).
        var getResult = exprRef.Value.GetValue(vm);
        if (getResult.IsAbruptCompletion()) return getResult;

        var exprValue = getResult.Value.ToBoolean();

        // 3. If exprValue is true, then
        Completion stmtCompletion;
        if (exprValue.Value)
        {
            // a. Let stmtCompletion be Completion(Evaluation of the first Statement).
            stmtCompletion = IfCaseStatement.Evaluate(vm);
        }
        // 4. Else,
        else
        {
            // a. Let stmtCompletion be Completion(Evaluation of the second Statement).
            stmtCompletion = ElseCaseStatement!.Evaluate(vm);
        }

        // 5. Return ? UpdateEmpty(stmtCompletion, undefined).
        stmtCompletion.UpdateEmpty(Undefined.The);
        return stmtCompletion;
    }

    public IExpression IfExpression { get; }
    public INode IfCaseStatement { get; }
    public INode? ElseCaseStatement { get; }
}
