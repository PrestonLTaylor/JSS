namespace JSS.Lib.AST;

// 13.1 Identifiers, https://tc39.es/ecma262/#sec-identifiers
internal sealed class Identifier : IExpression
{
    public Identifier(string name)
    {
        Name = name;
    }

    // 13.1.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-identifiers-runtime-semantics-evaluation

    public string Name { get; }
}
