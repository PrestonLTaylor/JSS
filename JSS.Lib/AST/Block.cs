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

        // 3. Perform BlockDeclarationInstantiation(StatementList, blockEnv).
        BlockDeclarationInstantiation(blockEnv);

        // 4. Set the running execution context's LexicalEnvironment to blockEnv.
        currentExecutionContext.LexicalEnvironment = blockEnv;

        // 5. Let blockValue be Completion(Evaluation of StatementList).
        var blockValue = Statements.Evaluate(vm);

        // 6. Set the running execution context's LexicalEnvironment to oldEnv.
        currentExecutionContext.LexicalEnvironment = oldEnv;

        // 7. Return ? blockValue.
        return blockValue;
    }

    // 14.2.3 BlockDeclarationInstantiation ( code, env ), https://tc39.es/ecma262/#sec-blockdeclarationinstantiation
    private void BlockDeclarationInstantiation(DeclarativeEnvironment env)
    {
        // 1. Let declarations be the LexicallyScopedDeclarations of code.
        var declarations = Statements.LexicallyScopedDeclarations();

        // FIXME: 2. Let privateEnv be the running execution context's PrivateEnvironment.

        // 3. For each element d of declarations, do
        foreach (var d in declarations)
        {
            // a. For each element dn of the BoundNames of d, do
            foreach (var dn in d.BoundNames())
            {
                // i. If IsConstantDeclaration of d is true, then
                if (d is ConstDeclaration)
                {
                    // 1. Perform ! env.CreateImmutableBinding(dn, true).
                    MUST(env.CreateImmutableBinding(dn, true));
                }
                // ii. Else,
                else
                {
                    // 1. Perform ! env.CreateMutableBinding(dn, false). NOTE: This step is replaced in section B.3.2.6.
                    MUST(env.CreateMutableBinding(dn, true));
                }
            }

            // b. If d is either a FunctionDeclaration, FIXME: a GeneratorDeclaration, an AsyncFunctionDeclaration, or an AsyncGeneratorDeclaration, then
            if (d is FunctionDeclaration)
            {
                // i. Let fn be the sole element of the BoundNames of d.
                var fn = d.BoundNames().FirstOrDefault()!;

                // ii. Let fo be InstantiateFunctionObject of d with arguments env and privateEnv.
                var f = d as FunctionDeclaration;
                var fo = f!.InstantiateFunctionObject(env);

                // iii. Perform ! env.InitializeBinding(fn, fo). NOTE: This step is replaced in section B.3.2.6.
                MUST(env.InitializeBinding(fn, fo));
            }
        }

        // 4. Return unused.
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
