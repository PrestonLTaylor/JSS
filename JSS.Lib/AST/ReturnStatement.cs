using JSS.Lib.Execution;
using JSS.Lib.AST.Values;

namespace JSS.Lib.AST;

// 14.10 The return Statement, https://tc39.es/ecma262/#sec-return-statement
internal sealed class ReturnStatement : INode
{
    public ReturnStatement(IExpression? returnExpression)
    {
        ReturnExpression = returnExpression;
    }

    // 14.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-return-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (ReturnExpression is null)
        {
            return EvaluateWithNoExpression();
        }
        else
        {
            return EvaluateWithExpression(vm);
        }
    }

    private Completion EvaluateWithNoExpression()
    {
        // 1. Return Completion Record { [[Type]]: RETURN, [[Value]]: undefined, [[Target]]: EMPTY }.
        return Completion.ReturnCompletion(Undefined.The);
    }

    private Completion EvaluateWithExpression(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = ReturnExpression!.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let exprValue be ? GetValue(exprRef).
        var exprValue = exprRef.Value.GetValue();
        if (exprValue.IsAbruptCompletion()) return exprValue;

        // FIXME: 3. If GetGeneratorKind() is ASYNC, set exprValue to ? Await(exprValue).

        // 4. Return Completion Record { [[Type]]: RETURN, [[Value]]: exprValue, [[Target]]: EMPTY }.
        return Completion.ReturnCompletion(exprValue.Value);
    }

    public IExpression? ReturnExpression { get; }
}
