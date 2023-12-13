namespace JSS.Lib.AST;

// 15.2 Function Definitions, https://tc39.es/ecma262/#prod-FunctionDeclaration
internal sealed class FunctionDeclaration : Declaration
{
    // FIXME: Early errors for parameters having the same name, https://tc39.es/ecma262/#sec-function-definitions-static-semantics-early-errors
    public FunctionDeclaration(string identifier, List<Identifier> parameters, StatementList body)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return the BoundNames of BindingIdentifier.
        return new List<string> { Identifier };
    }

    // FIXME: 15.2.6 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-definitions-runtime-semantics-evaluation

    public string Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public StatementList Body { get; }
}
