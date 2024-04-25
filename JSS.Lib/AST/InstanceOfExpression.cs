using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
internal sealed class InstanceOfExpression : IExpression
{
    public InstanceOfExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-relational-operators-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
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

        // 5. Return ? InstanceofOperator(lval, rval).
        return InstanceofOperator(vm, lval.Value, rval.Value);
    }

    // 13.10.2 InstanceofOperator ( V, target ), https://tc39.es/ecma262/#sec-instanceofoperator
    private Completion InstanceofOperator(VM vm, Value V, Value target)
    {
        // 1. If target is not an Object, throw a TypeError exception.
        if (!target.IsObject()) return ThrowTypeError(vm, RuntimeErrorType.RhsOfInstanceOfIsNotObject, target.Type());

        // FIXME: 2. Let instOfHandler be ? GetMethod(target, @@hasInstance).
        // FIXME: 3. If instOfHandler is not undefined, then
        // FIXME: a. Return ToBoolean(? Call(instOfHandler, target, « V »)).
        // FIXME: 4. If IsCallable(target) is false, throw a TypeError exception.

        // 5. Return ? OrdinaryHasInstance(target, V).
        return target.OrdinaryHasInstance(vm, V);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
