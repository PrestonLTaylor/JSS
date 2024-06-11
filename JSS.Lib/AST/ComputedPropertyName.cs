using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// ComputedPropertyName, https://tc39.es/ecma262/#prod-ComputedPropertyName
internal sealed class ComputedPropertyName : INode
{
    public ComputedPropertyName(IExpression expression)
    {
        Expression = expression;
    }

    // 13.2.5.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-object-initializer-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let exprValue be ? Evaluation of AssignmentExpression.
        var exprValue = Expression.Evaluate(vm);
        if (exprValue.IsAbruptCompletion()) return exprValue;

        // 2. Let propName be ? GetValue(exprValue).
        var propName = exprValue.Value.GetValue(vm);
        if (propName.IsAbruptCompletion()) return propName;

        // 3. Return ? ToPropertyKey(propName).
        return propName.Value.ToPropertyKey(vm);
    }

    public IExpression Expression { get; }
}
