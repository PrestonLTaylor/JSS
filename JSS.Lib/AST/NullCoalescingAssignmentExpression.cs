using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.15 Assignment Operators, https://tc39.es/ecma262/#sec-assignment-operators
internal sealed class NullCoalescingAssignmentExpression : IExpression
{
    public NullCoalescingAssignmentExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.15.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-assignment-operators-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of LeftHandSideExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue(vm);
        if (lval.IsAbruptCompletion()) return lval;

        // 3. If lval is neither undefined nor null, return lval.
        if (!lval.Value.IsUndefined() && !lval.Value.IsNull()) return lval;

        // FIXME: 4. If IsAnonymousFunctionDefinition(AssignmentExpression) is true and IsIdentifierRef of LeftHandSideExpression is true, then
        // FIXME: a. Let rval be ? NamedEvaluation of AssignmentExpression with argument lref.[[ReferencedName]].
        // FIXME: 5. Else,

        // a. Let rref be ? Evaluation of AssignmentExpression.
        var rref = Rhs.Evaluate(vm);

        // b. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue(vm);
        if (rval.IsAbruptCompletion()) return rval;

        // 6. Perform ? PutValue(lref, rval).
        var putResult = lref.Value.PutValue(vm, rval.Value);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 7. Return rval.
        return rval;
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
