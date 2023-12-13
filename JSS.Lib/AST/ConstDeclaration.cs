namespace JSS.Lib.AST;

// 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
internal sealed class ConstDeclaration : Declaration
{
    public ConstDeclaration(string identifier, INode initializer)
    {
        Identifier = identifier;
        Initializer = initializer;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return the BoundNames of BindingIdentifier.
        return new List<string> { Identifier };
    }

    // FIXME: 14.3.1.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-let-and-const-declarations-runtime-semantics-evaluation

    public string Identifier { get; }

    // FIXME: Maybe have a more granular class for "AssignmentExpression"s: https://tc39.es/ecma262/#prod-AssignmentExpression
    public INode Initializer { get; }
}
