using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal sealed class VarStatement : INode
{
    public VarStatement(List<VarDeclaration> declarations)
    {
        Declarations = declarations;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Let names1 be BoundNames of VariableDeclarationList.
        // 2. Let names2 be BoundNames of VariableDeclaration.
        List<string> boundNames = new();
        foreach (var declaration in Declarations)
        {
            boundNames.AddRange(declaration.BoundNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return boundNames;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Return BoundNames of VariableDeclarationList.
        return BoundNames();
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be VarScopedDeclarations of VariableDeclarationList.
        // 2. Return the list-concatenation of declarations1 and « VariableDeclaration ».
        return new(Declarations);
    }

    // 14.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-variable-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Perform ? Evaluation of VariableDeclarationList.
        foreach (var declaration in Declarations)
        {
            var result = declaration.Evaluate(vm);
            if (result.IsAbruptCompletion()) return result;
        }

        // 2. Return EMPTY.
        return Empty.The;
    }

    public IReadOnlyList<VarDeclaration> Declarations { get; }
}
