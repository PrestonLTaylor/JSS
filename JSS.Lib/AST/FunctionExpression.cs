namespace JSS.Lib.AST;

// 15.2 Function Definitions, https://tc39.es/ecma262/#sec-function-definitions
internal sealed class FunctionExpression : IExpression
{
    public FunctionExpression(string? identifier, List<Identifier> parameters, StatementList body)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
    }

    // FIXME: 15.2.6 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-definitions-runtime-semantics-evaluation

    public string? Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public StatementList Body { get; }
}
