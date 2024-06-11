using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.15 Assignment Operators, https://tc39.es/ecma262/#sec-assignment-operators
internal sealed class LogicalAndAssignmentExpression : IExpression
{
    public LogicalAndAssignmentExpression(IExpression lhs, IExpression rhs)
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

        // 1. Let lref be ? Evaluation of LeftHandSideExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue(vm);
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let lbool be ToBoolean(lval).
        var lbool = lval.Value.ToBoolean();

        // 4. If lbool is false, return lval.
        if (!lbool.Value)
        {
            return lval;
        }

        // FIXME: 5. If IsAnonymousFunctionDefinition(AssignmentExpression) is true and IsIdentifierRef of LeftHandSideExpression is true, then
        // FIXME: a. Let rval be ? NamedEvaluation of AssignmentExpression with argument lref.[[ReferencedName]].
        // FIXME: 6. Else,

        // a. Let rref be ? Evaluation of AssignmentExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // b. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue(vm);
        if (rval.IsAbruptCompletion()) return rval;

        // 7. Perform ? PutValue(lref, rval).
        var putResult = lref.Value.PutValue(vm, rval.Value);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 8. Return rval.
        return rval;
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
