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

    // FIXME: 15.7.16 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-class-definitions-runtime-semantics-evaluation

    public string Identifier { get; }
    public IReadOnlyList<MethodDeclaration> Methods { get; }
    public IReadOnlyList<MethodDeclaration> StaticMethods { get; }
    public IReadOnlyList<FieldDeclaration> Fields { get; }
    public IReadOnlyList<FieldDeclaration> StaticFields { get; }
}
