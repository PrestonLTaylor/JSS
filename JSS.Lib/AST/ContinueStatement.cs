namespace JSS.Lib.AST;

// 14.8 The continue Statement, https://tc39.es/ecma262/#prod-ContinueStatement
internal sealed class ContinueStatement : INode
{
    public ContinueStatement(Identifier? label)
    {
        Label = label;
    }

    // FIXME: 14.8.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-continue-statement-runtime-semantics-evaluation

    public Identifier? Label { get; }
}
