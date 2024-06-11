using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
internal sealed class InExpression : IExpression
{
    public InExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-relational-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let lref be ? Evaluation of RelationalExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue(vm);
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of ShiftExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue(vm);
        if (rval.IsAbruptCompletion()) return rval;

        // 5. If rval is not an Object, throw a TypeError exception.
        if (!rval.Value.IsObject())
        {
            return ThrowTypeError(vm, RuntimeErrorType.RhsOfInIsNotObject, rval.Value.Type());
        }

        // 6. Return ? HasProperty(rval, ? ToPropertyKey(lval)).
        var asObject = rval.Value.AsObject();

        var propertyKey = lval.Value.ToPropertyKey(vm);
        if (propertyKey.IsAbruptCompletion()) return propertyKey;

        // FIXME: We should be able to handle Symbols
        var propertyKeyAsString = propertyKey.Value.AsString();

        return Object.HasProperty(asObject, propertyKeyAsString.Value);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
