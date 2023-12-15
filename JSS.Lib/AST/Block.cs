using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.2 Block, https://tc39.es/ecma262/#sec-block
internal sealed class Block : INode 
{
    public Block(StatementList statements)
    {
        Statements = statements;
    }

    // 14.2.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let oldEnv be the running execution context's LexicalEnvironment.
        var currentExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
        var oldEnv = currentExecutionContext.LexicalEnvironment;

        // 2. Let blockEnv be NewDeclarativeEnvironment(oldEnv).
        var blockEnv = new DeclarativeEnvironment(oldEnv);

        // FIXME: 3. Perform BlockDeclarationInstantiation(StatementList, blockEnv).

        // 4. Set the running execution context's LexicalEnvironment to blockEnv.
        currentExecutionContext.LexicalEnvironment = blockEnv;

        // 5. Let blockValue be Completion(Evaluation of StatementList).
        var blockValue = Statements.Evaluate(vm);

        // 6. Set the running execution context's LexicalEnvironment to oldEnv.
        currentExecutionContext.LexicalEnvironment = oldEnv;

        // 7. Return ? blockValue.
        return blockValue;
    }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        // 1. Return a new empty List.
        return new List<string>();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Return a new empty List.
        return new List<string>();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Return a new empty List.
        return new List<INode>();
    }

    public StatementList Statements { get; }
}
