namespace JSS.Lib.AST;

// FieldDeclaration, https://tc39.es/ecma262/#prod-FieldDefinition
internal sealed class FieldDeclaration : INode
{
    public FieldDeclaration(string identifier, INode? initializer, bool isPrivate)
    {
        Identifier = identifier;
        Initializer = initializer;
        IsPrivate = isPrivate;
    }

    // FIXME: 15.7.10 Runtime Semantics: ClassFieldDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-classfielddefinitionevaluation
    public void Execute() { }

    public string Identifier { get; }
    public INode? Initializer { get; }
    public bool IsPrivate { get; }
}
