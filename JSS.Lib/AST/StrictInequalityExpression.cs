using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;

namespace JSS.Lib.AST;

// 13.11 Equality Operators, https://tc39.es/ecma262/#sec-equality-operators
internal sealed class StrictInequalityExpression : IExpression
{
    public StrictInequalityExpression(IExpression lhs, IExpression rhs)
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
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of RelationalExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue();
        if (rval.IsAbruptCompletion()) return rval;

        // 5. Let r be IsStrictlyEqual(rval, lval).
        var r = Value.IsStrictlyEqual(rval.Value, lval.Value);

        // 6. If r is true, return false. Otherwise, return true.
        return Completion.NormalCompletion(new Boolean(!r.Value));
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
