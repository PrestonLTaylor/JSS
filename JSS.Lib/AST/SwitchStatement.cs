using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal readonly struct CaseBlock
{
    public CaseBlock(IExpression caseExpression, StatementList statements)
    {
        CaseExpression = caseExpression;
        Statements = statements;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    public List<string> LexicallyDeclaredNames()
    {
        // 1. If the StatementList is present, return the LexicallyDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return Statements.LexicallyDeclaredNames();
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    public List<INode> LexicallyScopedDeclarations()
    {
        // 1. If the StatementList is present, return the LexicallyScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return Statements.LexicallyScopedDeclarations();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    public List<string> VarDeclaredNames()
    {
        // 1. If the StatementList is present, return the VarDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return Statements.VarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    public List<INode> VarScopedDeclarations()
    {
        // 1. If the StatementList is present, return the VarScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return Statements.VarScopedDeclarations();
    }

    public IExpression CaseExpression { get; }
    public IReadOnlyList<INode> StatementList 
    { 
        get { return Statements.Statements; }
    }
    public StatementList Statements { get; }
}

internal readonly struct DefaultBlock
{
    public DefaultBlock(StatementList statements)
    {
        Statements = statements;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    public List<string> LexicallyDeclaredNames()
    {
        // 1. If the StatementList is present, return the LexicallyDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return Statements.LexicallyDeclaredNames();
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    public List<INode> LexicallyScopedDeclarations()
    {
        // 1. If the StatementList is present, return the LexicallyScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return Statements.LexicallyScopedDeclarations();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    public List<string> VarDeclaredNames()
    {
        // 1. If the StatementList is present, return the VarDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return Statements.VarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    public List<INode> VarScopedDeclarations()
    {
        // 1. If the StatementList is present, return the VarScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return Statements.VarScopedDeclarations();
    }

    public IReadOnlyList<INode> StatementList 
    { 
        get { return Statements.Statements; }
    }
    public StatementList Statements { get; }
}

// 14.12 The switch Statement, https://tc39.es/ecma262/#sec-switch-statement
internal sealed class SwitchStatement : INode
{
    public SwitchStatement(IExpression switchExpression, List<CaseBlock> firstCaseBlocks, List<CaseBlock> secondCaseBlocks, DefaultBlock? defaultCase)
    {
        SwitchExpression = switchExpression;
        FirstCaseBlocks = firstCaseBlocks;
        SecondCaseBlocks = secondCaseBlocks;
        DefaultCase = defaultCase;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        List<string> names = new();

        // 1. If the first CaseClauses is present, let names1 be the LexicallyDeclaredNames of the first CaseClauses.
        // 2. Else, let names1 be a new empty List.
        foreach (var caseBlock in FirstCaseBlocks)
        {
            names.AddRange(caseBlock.LexicallyDeclaredNames());
        }

        // 3. Let names2 be LexicallyDeclaredNames of DefaultClause.
        if (DefaultCase is not null)
        {
            names.AddRange(DefaultCase.Value.LexicallyDeclaredNames());
        }

        // 4. If the second CaseClauses is present, let names3 be the LexicallyDeclaredNames of the second CaseClauses.
        // 5. Else, let names3 be a new empty List.
        foreach (var caseBlock in SecondCaseBlocks)
        {
            names.AddRange(caseBlock.LexicallyDeclaredNames());
        }

        // 6. Return the list-concatenation of names1, names2, and names3.
        return names;
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    override public List<INode> LexicallyScopedDeclarations()
    {
        List<INode> declarations = new();

        // 1. If the first CaseClauses is present, let declarations1 be the LexicallyScopedDeclarations of the first CaseClauses.
        // 2. Else, let declarations1 be a new empty List.
        foreach (var caseBlock in FirstCaseBlocks)
        {
            declarations.AddRange(caseBlock.LexicallyScopedDeclarations());
        }

        // 3. Let declarations2 be LexicallyScopedDeclarations of DefaultClause.
        if (DefaultCase is not null)
        {
            declarations.AddRange(DefaultCase.Value.LexicallyScopedDeclarations());
        }

        // 4. If the second CaseClauses is present, let declarations3 be the LexicallyScopedDeclarations of the second CaseClauses.
        // 5. Else, let declarations3 be a new empty List.
        foreach (var caseBlock in SecondCaseBlocks)
        {
            declarations.AddRange(caseBlock.LexicallyScopedDeclarations());
        }

        // 6. Return the list-concatenation of declarations1, declarations2, and declarations3.
        return declarations;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        List<string> names = new();

        // 1. If the first CaseClauses is present, let names1 be the VarDeclaredNames of the first CaseClauses.
        // 2. Else, let names1 be a new empty List.
        foreach (var caseBlock in FirstCaseBlocks)
        {
            names.AddRange(caseBlock.VarDeclaredNames());
        }

        // 3. Let names2 be VarDeclaredNames of DefaultClause.
        if (DefaultCase is not null)
        {
            names.AddRange(DefaultCase.Value.VarDeclaredNames());
        }

        // 4. If the second CaseClauses is present, let names3 be the VarDeclaredNames of the second CaseClauses.
        // 5. Else, let names3 be a new empty List.
        foreach (var caseBlock in SecondCaseBlocks)
        {
            names.AddRange(caseBlock.VarDeclaredNames());
        }

        // 6. Return the list-concatenation of names1, names2, and names3.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        List<INode> declarations = new();

        // 1. If the first CaseClauses is present, let declarations1 be the VarScopedDeclarations of the first CaseClauses.
        // 2. Else, let declarations1 be a new empty List.
        foreach (var caseBlock in FirstCaseBlocks)
        {
            declarations.AddRange(caseBlock.VarScopedDeclarations());
        }

        // 3. Let declarations2 be VarScopedDeclarations of DefaultClause.
        if (DefaultCase is not null)
        {
            declarations.AddRange(DefaultCase.Value.VarScopedDeclarations());
        }

        // 4. If the second CaseClauses is present, let declarations3 be the VarScopedDeclarations of the second CaseClauses.
        // 5. Else, let declarations3 be a new empty List.
        foreach (var caseBlock in SecondCaseBlocks)
        {
            declarations.AddRange(caseBlock.VarScopedDeclarations());
        }

        // 6. Return the list-concatenation of declarations1, declarations2, and declarations3.
        return declarations;
    }

    // 14.12.2 Runtime Semantics: CaseBlockEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-caseblockevaluation
    private Completion CaseBlockEvaluation(VM vm, Value input)
    {
        if (DefaultCase is null)
        {
            return CaseBlockEvaluationWithoutDefault(vm, input);
        }
        else
        {
            return CaseBlockEvaluationWithDefault(vm, input);
        }
    }

    private Completion CaseBlockEvaluationWithoutDefault(VM vm, Value input)
    {
        // 1. Let V be undefined.
        Value V = Undefined.The;

        // 2. Let A be the List of CaseClause items in CaseClauses, in source text order.
        // 3. Let found be false.
        var found = false;

        // NOTE: In this case all the case blocks are in FirstCaseBlocks
        Assert(SecondCaseBlocks.Count == 0, "Case statement without default has case blocks in the SecondCaseBlocks list.");

        // 4. For each CaseClause C of A, do
        foreach (var C in FirstCaseBlocks)
        {
            // a. If found is false, then
            if (!found)
            {
                // i. Set found to ? CaseClauseIsSelected(C, input).
                var selectedResult = CaseClauseIsSelected(vm, C, input);
                if (selectedResult.IsAbruptCompletion()) return selectedResult.Completion;
                found = selectedResult.Value;
            }

            // b. If found is true, then
            if (found)
            {
                // i. Let R be Completion(Evaluation of C).
                var R = C.Statements.Evaluate(vm);

                // ii. If R.[[Value]] is not EMPTY, set V to R.[[Value]].
                if (!R.IsValueEmpty())
                {
                    V = R.Value;
                }

                // iii. If R is an abrupt completion, return ? UpdateEmpty(R, V).
                if (R.IsAbruptCompletion())
                {
                    R.UpdateEmpty(V);
                    return R;
                }
            }
        }

        // 5. Return V.
        return V;
    }

    private Completion CaseBlockEvaluationWithDefault(VM vm, Value input)
    {
        // 1. Let V be undefined.
        Value V = Undefined.The;

        // 2. If the first CaseClauses is present, then
        // a. Let A be the List of CaseClause items in the first CaseClauses, in source text order.
        // 3. Else,
        // a. Let A be a new empty List.

        // 4. Let found be false.
        var found = false;

        // 5. For each CaseClause C of A, do
        foreach (var C in FirstCaseBlocks)
        {
            // a. If found is false, then
            if (!found)
            {
                // i. Set found to ? CaseClauseIsSelected(C, input).
                var selectedResult = CaseClauseIsSelected(vm, C, input);
                if (selectedResult.IsAbruptCompletion()) return selectedResult.Completion;
                found = selectedResult.Value;
            }

            // b. If found is true, then
            if (found)
            {
                // i. Let R be Completion(Evaluation of C).
                var R = C.Statements.Evaluate(vm);

                // ii. If R.[[Value]] is not EMPTY, set V to R.[[Value]].
                if (!R.IsValueEmpty())
                {
                    V = R.Value;
                }

                // iii. If R is an abrupt completion, return ? UpdateEmpty(R, V).
                if (R.IsAbruptCompletion())
                {
                    R.UpdateEmpty(V);
                    return R;
                }
            }
        }

        // 6. Let foundInB be false.
        var foundInB = false;

        // 7. If the second CaseClauses is present, then
        // a. Let B be the List of CaseClause items in the second CaseClauses, in source text order.
        // 8. Else,
        // a. Let B be a new empty List.

        // 9. If found is false, then
        if (!found)
        {
            // a. For each CaseClause C of B, do
            foreach (var C in SecondCaseBlocks)
            {
                // i. If foundInB is false, then
                if (!foundInB)
                {
                    // 1. Set foundInB to ? CaseClauseIsSelected(C, input).
                    var selectedResult = CaseClauseIsSelected(vm, C, input);
                    if (selectedResult.IsAbruptCompletion()) return selectedResult.Completion;
                    foundInB = selectedResult.Value;
                }

                // ii. If foundInB is true, then
                if (foundInB)
                {
                    // 1. Let R be Completion(Evaluation of CaseClause C).
                    var R = C.Statements.Evaluate(vm);

                    // 2. If R.[[Value]] is not EMPTY, set V to R.[[Value]].
                    if (!R.IsValueEmpty())
                    {
                        V = R.Value;
                    }

                    // 3. If R is an abrupt completion, return ? UpdateEmpty(R, V).
                    if (R.IsAbruptCompletion())
                    {
                        R.UpdateEmpty(V);
                        return R;
                    }
                }
            }
        }

        // 10. If foundInB is true, return V.
        if (foundInB) return V;

        // 11. Let defaultR be Completion(Evaluation of DefaultClause).
        var defaultR = DefaultCase!.Value.Statements.Evaluate(vm);

        // 12. If defaultR.[[Value]] is not EMPTY, set V to defaultR.[[Value]].
        if (!defaultR.IsValueEmpty())
        {
            V = defaultR.Value;
        }

        // 13. If defaultR is an abrupt completion, return ? UpdateEmpty(defaultR, V).
        if (defaultR.IsAbruptCompletion())
        {
            defaultR.UpdateEmpty(V);
            return defaultR;
        }

        // 14. NOTE: The following is another complete iteration of the second CaseClauses.

        // 15. For each CaseClause C of B, do
        foreach (var C in SecondCaseBlocks)
        {
            // a. Let R be Completion(Evaluation of CaseClause C).
            var R = C.Statements.Evaluate(vm);

            // b. If R.[[Value]] is not EMPTY, set V to R.[[Value]].
            if (!R.IsValueEmpty())
            {
                V = R.Value;
            }

            // c. If R is an abrupt completion, return ? UpdateEmpty(R, V).
            if (R.IsAbruptCompletion())
            {
                R.UpdateEmpty(V);
                return R;
            }
        }

        // 16. Return V.
        return V;
    }

    private AbruptOr<Boolean> CaseClauseIsSelected(VM vm, CaseBlock C, Value input)
    {
        // 1. Assert: C is an instance of the production CaseClause : case Expression : StatementListopt.

        // 2. Let exprRef be ? Evaluation of the Expression of C.
        var exprRef = C.CaseExpression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 3. Let clauseSelector be ? GetValue(exprRef).
        var clauseSelector = exprRef.Value.GetValue(vm);
        if (clauseSelector.IsAbruptCompletion()) return clauseSelector;

        // 4. Return IsStrictlyEqual(input, clauseSelector).
        return Value.IsStrictlyEqual(input, clauseSelector.Value);
    }

    // 14.12.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-switch-statement-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let exprRef be ? Evaluation of Expression.
        var exprRef = SwitchExpression.Evaluate(vm);
        if (exprRef.IsAbruptCompletion()) return exprRef;

        // 2. Let switchValue be ? GetValue(exprRef).
        var switchValue = exprRef.Value.GetValue(vm);
        if (switchValue.IsAbruptCompletion()) return switchValue;

        // 3. Let oldEnv be the running execution context's LexicalEnvironment.
        var executionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
        var oldEnv = executionContext.LexicalEnvironment;

        // 4. Let blockEnv be NewDeclarativeEnvironment(oldEnv).
        var blockEnv = new DeclarativeEnvironment(oldEnv);

        // 5. Perform BlockDeclarationInstantiation(CaseBlock, blockEnv).
        Block.BlockDeclarationInstantiation(vm, this, blockEnv);

        // 6. Set the running execution context's LexicalEnvironment to blockEnv.
        executionContext.LexicalEnvironment = blockEnv;

        // 7. Let R be Completion(CaseBlockEvaluation of CaseBlock with argument switchValue).
        var R = CaseBlockEvaluation(vm, switchValue.Value);

        // 8. Set the running execution context's LexicalEnvironment to oldEnv.
        executionContext.LexicalEnvironment = oldEnv;

        // FIXME/NOTE: I'm not sure if this is a spec bug (probably not) or a bug in our code, but if we have a break completion here,
        // we'll return the break completion and not execute anything else.
        if (R.IsBreakCompletion()) R = R.Value;

        // 9. Return R.
        return R;
    }

    public IExpression SwitchExpression { get; }
    public IReadOnlyList<CaseBlock> FirstCaseBlocks { get; }
    public DefaultBlock? DefaultCase { get; }
    public IReadOnlyList<CaseBlock> SecondCaseBlocks { get; }
}
