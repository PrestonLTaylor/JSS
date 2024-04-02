using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.15 Assignment Operators, https://tc39.es/ecma262/#sec-assignment-operators
internal sealed class BinaryOpAssignmentExpression : IExpression
{
    public BinaryOpAssignmentExpression(IExpression lhs, BinaryOpType op, IExpression rhs)
    {
        Lhs = lhs;
        Op = op;
        Rhs = rhs;
    }

    // FIXME: 13.15.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-assignment-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of LeftHandSideExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of AssignmentExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue();
        if (rval.IsAbruptCompletion()) return rval;

        // 5. Let assignmentOpText be the source text matched by AssignmentOperator.
        // 6. Let opText be the sequence of Unicode code points associated with assignmentOpText in the following table:
        // 7. Let r be ? ApplyStringOrNumericBinaryOperator(lval, opText, rval).
        var r = ApplyStringOrNumericBinaryOperator(lval.Value, Op, rval.Value);
        if (r.IsAbruptCompletion()) return r;

        // 8. Perform ? PutValue(lref, r).
        var putResult = lref.Value.PutValue(vm, r.Value);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 9. Return r.
        return r;
    }

    public IExpression Lhs { get; }
    public BinaryOpType Op { get; }
    public IExpression Rhs { get; }
}
