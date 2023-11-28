using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.14 The throw Statement, https://tc39.es/ecma262/#sec-throw-statement
internal sealed class ThrowStatement : INode
{
    public ThrowStatement(IExpression throwExpression)
    {
        ThrowExpression = throwExpression;
    }

    // 14.14.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-throw-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = ThrowExpression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let exprValue be ? GetValue(exprRef).
        var exprValue = exprRef.Value.GetValue();
        if (exprValue.IsAbruptCompletion()) return exprValue;

        // 3. Return ThrowCompletion(exprValue).
        return Completion.ThrowCompletion(exprValue.Value);
    }

    public IExpression ThrowExpression { get; }
}
