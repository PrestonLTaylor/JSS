using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 15.3 Arrow Function Definitions, https://tc39.es/ecma262/#sec-arrow-function-definitions
internal sealed class ExpressionBody : INode
{
    public ExpressionBody(IExpression expression)
    {
        Expression = expression;
    }

    // 15.3.5 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-arrow-function-definitions-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of AssignmentExpression.
        var exprRef = Expression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let exprValue be ? GetValue(exprRef).
        var exprValue = exprRef.Value.GetValue(vm);
        if (exprValue.IsAbruptCompletion()) return exprValue;

        // 3. Return Completion Record { [[Type]]: return, [[Value]]: exprValue, [[Target]]: empty }.
        return Completion.ReturnCompletion(exprValue.Value);
    }

    public IExpression Expression { get; }
}
