using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.11 Equality Operators, https://tc39.es/ecma262/#sec-equality-operators
internal sealed class LooseInequalityExpression : IExpression
{
    public LooseInequalityExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.11.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-equality-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of EqualityExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue(vm);
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of RelationalExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue(vm);
        if (rval.IsAbruptCompletion()) return rval;

        // 5. Let r be ? IsLooselyEqual(rval, lval).
        var r = Value.IsLooselyEqual(vm, rval.Value, lval.Value);
        if (r.IsAbruptCompletion()) return r;

        // 6. If r is true, return false. Otherwise, return true.
        var rAsBoolean = r.Value.AsBoolean().Value;
        return !rAsBoolean;
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
