namespace JSS.Lib.AST;

// 15.7 Class Definitions, https://tc39.es/ecma262/#sec-class-definitions
internal sealed class ClassDeclaration : Declaration
{
    public ClassDeclaration(string identifier, List<MethodDeclaration> methods, List<MethodDeclaration> staticMethods, List<FieldDeclaration> fields,
        List<FieldDeclaration> staticFields)
    {
        Identifier = identifier;
        Methods = methods;
        StaticMethods = staticMethods;
        Fields = fields;
        StaticFields = staticFields;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return the BoundNames of BindingIdentifier.
        return new List<string> { Identifier };
    }

    // FIXME: 15.7.16 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-class-definitions-runtime-semantics-evaluation

    public string Identifier { get; }
    public IReadOnlyList<MethodDeclaration> Methods { get; }
    public IReadOnlyList<MethodDeclaration> StaticMethods { get; }
    public IReadOnlyList<FieldDeclaration> Fields { get; }
    public IReadOnlyList<FieldDeclaration> StaticFields { get; }
}
