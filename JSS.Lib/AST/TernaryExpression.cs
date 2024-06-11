using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.14 Conditional Operator ( ? : ), https://tc39.es/ecma262/#prod-ConditionalExpression
internal sealed class TernaryExpression : IExpression
{
    public TernaryExpression(IExpression testExpression, IExpression trueCaseExpression,  IExpression falseCaseExpression)
    {
        TestExpression = testExpression;
        TrueCaseExpression = trueCaseExpression;
        FalseCaseExpression = falseCaseExpression;
    }

    // 13.14.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#prod-ConditionalExpression
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let lref be ? Evaluation of ShortCircuitExpression.
        var lref = TestExpression.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ToBoolean(? GetValue(lref)).
        var getResult = lref.Value.GetValue(vm);
        if (getResult.IsAbruptCompletion()) return getResult;
        var lval = getResult.Value.ToBoolean();

        // 3. If lval is true, then
        if (lval)
        {
            // a. Let trueRef be ? Evaluation of the first AssignmentExpression.
            var trueRef = TrueCaseExpression.Evaluate(vm);
            if (trueRef.IsAbruptCompletion()) return trueRef;

            // b. Return ? GetValue(trueRef).
            return trueRef.Value.GetValue(vm);
        }
        // 4. Else,
        else
        {
            // a. Let falseRef be ? Evaluation of the second AssignmentExpression.
            var falseRef = FalseCaseExpression.Evaluate(vm);
            if (falseRef.IsAbruptCompletion()) return falseRef;

            // b. Return ? GetValue(falseRef).
            return falseRef.Value.GetValue(vm);
        }
    }

    public IExpression TestExpression { get; }
    public IExpression TrueCaseExpression { get; }
    public IExpression FalseCaseExpression { get; }
}
