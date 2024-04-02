﻿using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// LogicalANDExpression, https://tc39.es/ecma262/#prod-LogicalANDExpression
internal sealed class LogicalAndExpression : IExpression
{
    public LogicalAndExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.13.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-binary-logical-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of LogicalANDExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let lbool be ToBoolean(lval).
        var lbool = lval.Value.ToBoolean();

        // 4. If lbool is false, return lval.
        if (!lbool.Value) return lval;

        // 5. Let rref be ? Evaluation of BitwiseORExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 6. Return ? GetValue(rref).
        return rref.Value.GetValue();
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
