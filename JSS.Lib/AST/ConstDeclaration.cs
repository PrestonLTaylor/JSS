namespace JSS.Lib.AST;

// 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
internal sealed class ConstDeclaration : Declaration
{
    public ConstDeclaration(string identifier, INode initializer)
    {
        Identifier = identifier;
        Initializer = initializer;
    }

    // FIXME: 14.3.1.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-let-and-const-declarations-runtime-semantics-evaluation

    public string Identifier { get; }

    // FIXME: Maybe have a more granular class for "AssignmentExpression"s: https://tc39.es/ecma262/#prod-AssignmentExpression
    public INode Initializer { get; }
}
