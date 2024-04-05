using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
internal sealed class LessThanEqualsExpression : IExpression
{
    public LessThanEqualsExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-relational-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of RelationalExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of ShiftExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue();
        if (rval.IsAbruptCompletion()) return rval;

        // 5. Let r be ? IsLessThan(rval, lval, false).
        var r = Value.IsLessThan(rval.Value, lval.Value, false);
        if (r.IsAbruptCompletion()) return r;

        // 6. If r is either true or undefined, return false. Otherwise, return true.
        if (r.Value.IsUndefined()) return false;

        var rAsBoolean = r.Value.AsBoolean();
        return !rAsBoolean.Value;
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
