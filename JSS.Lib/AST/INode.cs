using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal abstract class INode
{
    virtual public Completion Evaluate(VM vm) { throw new NotImplementedException($"{GetType().Name}'s Evaluate is not yet implemented."); }

    // NOTE: If a Node doesn't override BoundNames, then it has no bound names
    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    virtual public List<string> BoundNames() { return new List<string>(); }

    // 8.2.4 Static Semantics: LexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-lexicallydeclarednames
    virtual public List<string> LexicallyDeclaredNames()
    {
        // FIXME: 1. If Statement is Statement : LabelledStatement, return LexicallyDeclaredNames of LabelledStatement.
        // 2. Return a new empty List.
        return new List<string>();
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    virtual public List<string> VarDeclaredNames()
    {
        // 1. Return a new empty List.
        return new List<string>();
    }

    // 8.2.8 Static Semantics: TopLevelLexicallyDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevellexicallydeclarednames
    virtual public List<string> TopLevelLexicallyDeclaredNames()
    {
        // 1. Return a new empty List.
        return new List<string>();
    }

    // 8.2.10 Static Semantics: TopLevelVarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-toplevelvardeclarednames
    virtual public List<string> TopLevelVarDeclaredNames()
    {
        // FIXME: 1. If Statement is Statement : LabelledStatement, return TopLevelVarDeclaredNames of Statement.
        // 2. Return VarDeclaredNames of Statement.
        return VarDeclaredNames();
    }
}
