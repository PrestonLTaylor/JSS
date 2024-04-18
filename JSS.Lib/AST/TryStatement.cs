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

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Let names1 be VarDeclaredNames of Block.
        List<string> names = new();
        names.AddRange(VarDeclaredNamesOfBlock());

        // 2. Let names2 be VarDeclaredNames of Catch.
        names.AddRange(VarDeclaredNamesOfCatch());

        // 3. Let names3 be VarDeclaredNames of Finally.
        names.AddRange(VarDeclaredNamesOfFinally());

        // 4. Return the list-concatenation of names1, names2, and names3.
        return names;
    }

    private List<string> VarDeclaredNamesOfBlock()
    {
        return TryBlock.VarDeclaredNames();
    }

    private List<string> VarDeclaredNamesOfCatch()
    {
        // 1. Return the VarDeclaredNames of Block.
        return CatchBlock?.VarDeclaredNames() ?? new();
    }

    private List<string> VarDeclaredNamesOfFinally()
    {
        return FinallyBlock?.VarDeclaredNames() ?? new();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be VarScopedDeclarations of Block.
        List<INode> declarations = new();
        declarations.AddRange(VarScopedDeclarationsOfBlock());

        // 2. Let declarations2 be VarScopedDeclarations of Catch.
        declarations.AddRange(VarScopedDeclarationsOfCatch());

        // 3. Let declarations3 be VarScopedDeclarations of Finally.
        declarations.AddRange(VarScopedDeclarationsOfFinally());

        // 4. Return the list-concatenation of declarations1, declarations2, and declarations3.
        return declarations;
    }

    private List<INode> VarScopedDeclarationsOfBlock()
    {
        return TryBlock.VarScopedDeclarations();
    }

    private List<INode> VarScopedDeclarationsOfCatch()
    {
        // 1. Return the VarDeclaredNames of Block.
        return CatchBlock?.VarScopedDeclarations() ?? new();
    }

    private List<INode> VarScopedDeclarationsOfFinally()
    {
        return FinallyBlock?.VarScopedDeclarations() ?? new();
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
        // 1. Let oldEnv be the running execution context's LexicalEnvironment.
        var currentExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
        var oldEnv = currentExecutionContext.LexicalEnvironment;

        // 2. Let catchEnv be NewDeclarativeEnvironment(oldEnv).
        var catchEnv = new DeclarativeEnvironment(oldEnv);

        // 3. For each element argName of the BoundNames of CatchParameter, do
        var boundNames = CatchParameter!.BoundNames();
        foreach (var argName in boundNames)
        {
            // a. Perform ! catchEnv.CreateMutableBinding(argName, false).
            MUST(catchEnv.CreateMutableBinding(argName, false));
        }

        // 4. Set the running execution context's LexicalEnvironment to catchEnv.
        currentExecutionContext.LexicalEnvironment = catchEnv;

        // 5. Let status be Completion(BindingInitialization of CatchParameter with arguments thrownValue and catchEnv).
        var status = CatchParameter.BindingInitialization(vm, thrownValue, catchEnv);

        // 6. If status is an abrupt completion, then
        if (status.IsAbruptCompletion())
        {
            // a. Set the running execution context's LexicalEnvironment to oldEnv.
            currentExecutionContext.LexicalEnvironment = oldEnv;

            // b. Return ? status.
            return status;
        }

        // 7. Let B be Completion(Evaluation of Block).
        var B = CatchBlock!.Evaluate(vm);

        // 8. Set the running execution context's LexicalEnvironment to oldEnv.
        currentExecutionContext.LexicalEnvironment = oldEnv;

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
        F.UpdateEmpty(Undefined.The);
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
        C.UpdateEmpty(Undefined.The);
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
        F.UpdateEmpty(Undefined.The);
        return F;
    }

    public Block TryBlock { get; }
    public Block? CatchBlock { get; }
    public Identifier? CatchParameter { get; }
    public Block? FinallyBlock { get; }
}
