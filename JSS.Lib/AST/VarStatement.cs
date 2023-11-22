namespace JSS.Lib.AST;

// 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
internal sealed class VarStatement : INode
{
    public VarStatement(string identifier, INode? initializer)
    {
        Identifier = identifier;
        Initializer = initializer;
    }

    // FIXME: 14.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-variable-statement-runtime-semantics-evaluation
    public void Execute() { }

    public string Identifier { get; }

    public INode? Initializer { get; }
}
