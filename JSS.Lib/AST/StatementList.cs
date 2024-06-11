using JSS.Lib.Execution;
using JSS.Lib.AST.Values;

namespace JSS.Lib.AST;

// StatementList, https://tc39.es/ecma262/#prod-StatementList
internal sealed class StatementList : INode
{
    public StatementList(List<INode> statements)
    {
        Statements = statements;
    }

    // 14.2.2 Runtime Semantics: Evaluation, StatementList, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let sl be ? Evaluation of StatementList.
        Completion completion = Empty.The;
        foreach (var statement in Statements)
        {
            // 2. Let s be Completion(Evaluation of StatementListItem).
            var s = statement.Evaluate(vm);

            // 3. Return ? UpdateEmpty(s, sl).
            s.UpdateEmpty(completion.Value);
            completion = s;
            if (completion.IsAbruptCompletion())
            {
                return completion;
            }
        }

        return completion;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        List<string> lexicallyDeclaredNames = new();

        // NOTE: This is the same as the list concatenation of each element's lexically declared names
        // 1. Let names1 be LexicallyDeclaredNames of StatementList.
        // 2. Let names2 be LexicallyDeclaredNames of StatementListItem.
        foreach (var statement in Statements)
        {
            lexicallyDeclaredNames.AddRange(statement.LexicallyDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return lexicallyDeclaredNames;
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    override public List<INode> LexicallyScopedDeclarations()
    {
        // 1. Let declarations1 be LexicallyScopedDeclarations of StatementList.
        // 2. Let declarations2 be LexicallyScopedDeclarations of StatementListItem.
        List<INode> declarations = new();

        foreach (var statement in Statements)
        {
            declarations.AddRange(statement.LexicallyScopedDeclarations());
        }

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Let names1 be VarDeclaredNames of StatementList.
        // 2. Let names2 be VarDeclaredNames of StatementListItem.
        List<string> names = new();
        foreach (var statement in Statements)
        {
            names.AddRange(statement.VarDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be VarScopedDeclarations of StatementList.
        // 2. Let declarations2 be VarScopedDeclarations of StatementListItem.
        List<INode> declarations = new();

        foreach (var statement in Statements)
        {
            declarations.AddRange(statement.VarScopedDeclarations());
        }

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 8.2.8 Static Semantics: TopLevelLexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevellexicallydeclarednames
    override public List<string> TopLevelLexicallyDeclaredNames()
    {
        // 1. Let names1 be TopLevelLexicallyDeclaredNames of StatementList.
        // 2. Let names2 be TopLevelLexicallyDeclaredNames of StatementListItem.
        List<string> names = new();
        foreach (var statement in Statements)
        {
            names.AddRange(statement.TopLevelLexicallyDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.9 Static Semantics: TopLevelLexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-toplevellexicallyscopeddeclarations
    override public List<INode> TopLevelLexicallyScopedDeclarations()
    {
        // 1. Let declarations1 be TopLevelLexicallyScopedDeclarations of StatementList.
        // 2. Let declarations2 be TopLevelLexicallyScopedDeclarations of StatementListItem.
        List<INode> declarations = new();

        foreach (var statement in Statements)
        {
            declarations.AddRange(statement.TopLevelLexicallyScopedDeclarations());
        }

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 8.2.10 Static Semantics: TopLevelVarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevelvardeclarednames
    override public List<string> TopLevelVarDeclaredNames()
    {
        // 1. Let names1 be TopLevelVarDeclaredNames of StatementList.
        // 2. Let names2 be TopLevelVarDeclaredNames of StatementListItem.
        List<string> names = new();

        foreach (var statement in Statements)
        {
            names.AddRange(statement.TopLevelVarDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.11 Static Semantics: TopLevelVarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-toplevelvarscopeddeclarations
    override public List<INode> TopLevelVarScopedDeclarations()
    {
        // 1. Let declarations1 be TopLevelVarScopedDeclarations of StatementList.
        // 2. Let declarations2 be TopLevelVarScopedDeclarations of StatementListItem.
        List<INode> declarations = new();
        foreach (var statement in Statements)
        {
            declarations.AddRange(statement.TopLevelVarScopedDeclarations());
        }

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    public IReadOnlyList<INode> Statements { get; }
}
