using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.15 Assignment Operators, https://tc39.es/ecma262/#sec-assignment-operators
internal sealed class BasicAssignmentExpression : IExpression
{
    public BasicAssignmentExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.15.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-assignment-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // FIXME: 1. If LeftHandSideExpression is neither an ObjectLiteral nor an ArrayLiteral, then

        // a. Let lref be ? Evaluation of LeftHandSideExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // FIXME: b. If IsAnonymousFunctionDefinition(AssignmentExpression) and IsIdentifierRef of LeftHandSideExpression are both true, then
        // FIXME: i. Let rval be ? NamedEvaluation of AssignmentExpression with argument lref.[[ReferencedName]].
        // FIXME: c. Else,

        // i. Let rref be ? Evaluation of AssignmentExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // ii. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue(vm);
        if (rval.IsAbruptCompletion()) return rval;

        // d. Perform ? PutValue(lref, rval).
        var putResult = lref.Value.PutValue(vm, rval.Value);
        if (putResult.IsAbruptCompletion()) return putResult;

        // e. Return rval.
        return rval;

        // FIXME: 2. Let assignmentPattern be the AssignmentPattern that is covered by LeftHandSideExpression.
        // FIXME: 3. Let rref be ? Evaluation of AssignmentExpression.
        // FIXME: 4. Let rval be ? GetValue(rref).
        // FIXME: 5. Perform ? DestructuringAssignmentEvaluation of assignmentPattern with argument rval.
        // FIXME: 6. Return rval.
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
