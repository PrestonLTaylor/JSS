namespace JSS.Lib.AST;

internal readonly struct CaseBlock
{
    public CaseBlock(IExpression caseExpression, StatementList statements)
    {
        CaseExpression = caseExpression;
        _statements = statements;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    public List<string> LexicallyDeclaredNames()
    {
        // 1. If the StatementList is present, return the LexicallyDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return _statements.LexicallyDeclaredNames();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    public List<string> VarDeclaredNames()
    {
        // 1. If the StatementList is present, return the VarDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return _statements.VarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    public List<INode> VarScopedDeclarations()
    {
        // 1. If the StatementList is present, return the VarScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return _statements.VarScopedDeclarations();
    }

    public IExpression CaseExpression { get; }
    public IReadOnlyList<INode> StatementList 
    { 
        get { return _statements.Statements; } 
    }
    private readonly StatementList _statements;
}

internal readonly struct DefaultBlock
{
    public DefaultBlock(StatementList statements)
    {
        _statements = statements;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    public List<string> LexicallyDeclaredNames()
    {
        // 1. If the StatementList is present, return the LexicallyDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return _statements.LexicallyDeclaredNames();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    public List<string> VarDeclaredNames()
    {
        // 1. If the StatementList is present, return the VarDeclaredNames of StatementList.
        // 2. Return a new empty List.
        return _statements.VarDeclaredNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    public List<INode> VarScopedDeclarations()
    {
        // 1. If the StatementList is present, return the VarScopedDeclarations of StatementList.
        // 2. Return a new empty List.
        return _statements.VarScopedDeclarations();
    }

    public IReadOnlyList<INode> StatementList 
    { 
        get { return _statements.Statements; } 
    }
    private readonly StatementList _statements;
}

// 14.12 The switch Statement, https://tc39.es/ecma262/#sec-switch-statement
internal sealed class SwitchStatement : INode
{
    public SwitchStatement(IExpression switchExpression, List<CaseBlock> caseBlocks, DefaultBlock? defaultCase)
    {
        SwitchExpression = switchExpression;
        CaseBlocks = caseBlocks;
        DefaultCase = defaultCase;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        // NOTE: We can't really do these steps precisely however, it will result in the same LexicallyDeclaredNames
        // 1. If the first CaseClauses is present, let names1 be the LexicallyDeclaredNames of the first CaseClauses.
        // 2. Else, let names1 be a new empty List.
        // 3. Let names2 be LexicallyDeclaredNames of DefaultClause.
        // 4. If the second CaseClauses is present, let names3 be the LexicallyDeclaredNames of the second CaseClauses.
        // 5. Else, let names3 be a new empty List.
        List<string> names = new();

        foreach (var caseBlock in CaseBlocks)
        {
            names.AddRange(caseBlock.LexicallyDeclaredNames());
        }

        if (DefaultCase is not null)
        {
            names.AddRange(DefaultCase.Value.LexicallyDeclaredNames());
        }

        // 6. Return the list-concatenation of names1, names2, and names3.
        return names;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. If the first CaseClauses is present, let names1 be the VarDeclaredNames of the first CaseClauses.
        // 2. Else, let names1 be a new empty List.
        // 3. Let names2 be VarDeclaredNames of DefaultClause.
        // 4. If the second CaseClauses is present, let names3 be the VarDeclaredNames of the second CaseClauses.
        // 5. Else, let names3 be a new empty List.
        List<string> names = new();

        foreach (var caseBlock in CaseBlocks)
        {
            names.AddRange(caseBlock.VarDeclaredNames());
        }

        if (DefaultCase is not null)
        {
            names.AddRange(DefaultCase.Value.VarDeclaredNames());
        }

        // 6. Return the list-concatenation of names1, names2, and names3.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. If the first CaseClauses is present, let declarations1 be the VarScopedDeclarations of the first CaseClauses.
        // 2. Else, let declarations1 be a new empty List.
        // 3. Let declarations2 be VarScopedDeclarations of DefaultClause.
        // 4. If the second CaseClauses is present, let declarations3 be the VarScopedDeclarations of the second CaseClauses.
        // 5. Else, let declarations3 be a new empty List.
        List<INode> declarations = new();

        foreach (var caseBlock in CaseBlocks)
        {
            declarations.AddRange(caseBlock.VarScopedDeclarations());
        }

        if (DefaultCase is not null)
        {
            declarations.AddRange(DefaultCase.Value.VarScopedDeclarations());
        }

        // 6. Return the list-concatenation of declarations1, declarations2, and declarations3.
        return declarations;
    }

    // FIXME: 14.12.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-switch-statement-runtime-semantics-evaluation

    public IExpression SwitchExpression { get; }
    public IReadOnlyList<CaseBlock> CaseBlocks { get; }
    public DefaultBlock? DefaultCase { get; }
}
