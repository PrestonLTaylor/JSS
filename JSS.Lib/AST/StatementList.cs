using JSS.Lib.Execution;

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
        // 1. Let sl be ? Evaluation of StatementList.
        Completion completion = Completion.NormalCompletion(vm.Empty);
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

    public IReadOnlyList<INode> Statements { get; }
}
