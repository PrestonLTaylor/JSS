﻿namespace JSS.Lib.AST;

internal class Declaration : INode
{
    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    override public List<string> LexicallyDeclaredNames()
    {
        // 1. Return the BoundNames of Declaration.
        return BoundNames();
    }

    // 8.2.5 Static Semantics: LexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-lexicallyscopeddeclarations
    override public List<INode> LexicallyScopedDeclarations()
    {
        // 1. Return a List whose sole element is DeclarationPart of Declaration.
        return new List<INode> { this };
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Return a new empty List.
        return new List<INode>();
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

    // 8.2.9 Static Semantics: TopLevelLexicallyScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-toplevellexicallyscopeddeclarations
    override public List<INode> TopLevelLexicallyScopedDeclarations()
    {
        // 1. If Declaration is Declaration : HoistableDeclaration , then
        if (IsHoistableDeclaration())
        {
            // a. Return a new empty List.
            return new();
        }

        // 2. Return « Declaration ».
        return new() { this };
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

    // 8.2.11 Static Semantics: TopLevelVarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-toplevelvarscopeddeclarations
    override public List<INode> TopLevelVarScopedDeclarations()
    {
        // 1. If Declaration is Declaration : HoistableDeclaration , then
        if (IsHoistableDeclaration())
        {
            // a. Let declaration be DeclarationPart of HoistableDeclaration.
            // b. Return « declaration ».
            return new List<INode> { this };
        }

        // 2. Return a new empty List.
        return new List<INode>();
    }

    private bool IsHoistableDeclaration()
    {
        // FIXME: Generator/Async/AsyncGenerator when we implement them
        return this is FunctionDeclaration;
    }
}
