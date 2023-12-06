using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.15 The try Statement, https://tc39.es/ecma262/#sec-try-statement
internal sealed class TryStatement : INode
{
    public TryStatement(Block tryBlock, Block? catchBlock, Identifier? catchParameter, Block? finallyBlock)
    {
        TryBlock = tryBlock;
        CatchBlock = catchBlock;
        CatchParameter = catchParameter;
        FinallyBlock = finallyBlock;
    }

    // 14.15.2 Runtime Semantics: CatchClauseEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-catchclauseevaluation
    private Completion CatchClauseEvaluation(VM vm, Value thrownValue)
    {
        if (CatchParameter is null)
        {
            return CatchClauseEvaluationWithoutParameter(vm);
        }
        else
        {
            return CatchClauseEvaluationWithParameter(vm, thrownValue);
        }
    }

    private Completion CatchClauseEvaluationWithoutParameter(VM vm)
    {
        // 1. Return ? Evaluation of Block.
        return CatchBlock!.Evaluate(vm);
    }

    private Completion CatchClauseEvaluationWithParameter(VM vm, Value thrownValue)
    {
        // FIXME: 1. Let oldEnv be the running execution context's LexicalEnvironment.
        // FIXME: 2. Let catchEnv be NewDeclarativeEnvironment(oldEnv).
        // FIXME: 3. For each element argName of the BoundNames of CatchParameter, do
        // FIXME: a. Perform ! catchEnv.CreateMutableBinding(argName, false).
        // FIXME: 4. Set the running execution context's LexicalEnvironment to catchEnv.
        // FIXME: 5. Let status be Completion(BindingInitialization of CatchParameter with arguments thrownValue and catchEnv).
        // FIXME: 6. If status is an abrupt completion, then
        // FIXME: a. Set the running execution context's LexicalEnvironment to oldEnv.
        // FIXME: b. Return ? status.

        // 7. Let B be Completion(Evaluation of Block).
        var B = CatchBlock!.Evaluate(vm);

        // FIXME: 8. Set the running execution context's LexicalEnvironment to oldEnv.

        // 9. Return ? B.
        return B;
    }

    // 14.15.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-try-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (CatchBlock is not null && FinallyBlock is not null)
        {
            return EvaluateWithCatchAndFinally(vm);
        }
        else if (CatchBlock is not null)
        {
            return EvaluateWithCatch(vm);
        }
        else if (FinallyBlock is not null)
        {
            return EvaluateWithFinally(vm);
        }

        throw new InvalidOperationException("Tried to execute a try statement with no catch or finally.");
    }

    private Completion EvaluateWithCatchAndFinally(VM vm)
    {
        // 1. Let B be Completion(Evaluation of Block).
        var B = TryBlock.Evaluate(vm);

        // 2. If B.[[Type]] is THROW, let C be Completion(CatchClauseEvaluation of Catch with argument B.[[Value]]).
        Completion C;
        if (B.IsThrowCompletion())
        {
            C = CatchClauseEvaluation(vm, B.Value);
        }
        // 3. Else, let C be B.
        else
        {
            C = B;
        }

        // 4. Let F be Completion(Evaluation of Finally).
        var F = FinallyBlock!.Evaluate(vm);

        // 5. If F.[[Type]] is NORMAL, set F to C.
        if (F.IsNormalCompletion())
        {
            F = C;
        }

        // 6. Return ? UpdateEmpty(F, undefined).
        F.UpdateEmpty(vm.Undefined);
        return F;
    }

    private Completion EvaluateWithCatch(VM vm)
    {
        // 1. Let B be Completion(Evaluation of Block).
        var B = TryBlock.Evaluate(vm);

        // 2. If B.[[Type]] is THROW, let C be Completion(CatchClauseEvaluation of Catch with argument B.[[Value]]).
        Completion C;
        if (B.IsThrowCompletion())
        {
            C = CatchClauseEvaluation(vm, B.Value);
        }
        // 3. Else, let C be B.
        else
        {
            C = B;
        }

        // 4. Return ? UpdateEmpty(C, undefined).
        C.UpdateEmpty(vm.Undefined);
        return C;
    }

    private Completion EvaluateWithFinally(VM vm)
    {
        // 1. Let B be Completion(Evaluation of Block).
        var B = TryBlock.Evaluate(vm);

        // 2. Let F be Completion(Evaluation of Finally).
        var F = FinallyBlock!.Evaluate(vm);

        // 3. If F.[[Type]] is NORMAL, set F to B.
        if (F.IsNormalCompletion())
        {
            F = B;
        }

        // 4. Return ? UpdateEmpty(F, undefined).
        F.UpdateEmpty(vm.Undefined);
        return F;
    }

    public Block TryBlock { get; }
    public Block? CatchBlock { get; }
    public Identifier? CatchParameter { get; }
    public Block? FinallyBlock { get; }
}
