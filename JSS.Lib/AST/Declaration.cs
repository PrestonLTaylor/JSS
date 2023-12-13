namespace JSS.Lib.AST;

internal class Declaration : INode
{
    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        // 1. Return the BoundNames of Declaration.
        return BoundNames();
    }

    // 8.2.8 Static Semantics: TopLevelLexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevellexicallydeclarednames
    override public List<string> TopLevelLexicallyDeclaredNames()
    {
        // 1. If Declaration is Declaration : HoistableDeclaration , then
        if (IsHoistableDeclaration())
        {
            // a. Return a new empty List.
            return new List<string>();
        }

        // 2. Return the BoundNames of Declaration.
        return BoundNames();
    }

    // 8.2.10 Static Semantics: TopLevelVarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevelvardeclarednames
    override public List<string> TopLevelVarDeclaredNames()
    {
        // 1. If Declaration is Declaration : HoistableDeclaration , then
        if (IsHoistableDeclaration())
        {
            // a. Return the BoundNames of HoistableDeclaration.
            return BoundNames();
        }

        // 2. Return a new empty List.
        return new List<string>();
    }

    private bool IsHoistableDeclaration()
    {
        // FIXME: Generator/Async/AsyncGenerator when we implement them
        return this is FunctionDeclaration;
    }
}
