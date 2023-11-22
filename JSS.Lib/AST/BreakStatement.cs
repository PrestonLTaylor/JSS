namespace JSS.Lib.AST;

// 14.9 The break Statement, https://tc39.es/ecma262/#sec-break-statement
internal sealed class BreakStatement : INode
{
    public BreakStatement(Identifier? label)
    {
        Label = label;
    }

    // FIXME: 14.9.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-break-statement-runtime-semantics-evaluation
    public void Execute() { }

    public Identifier? Label { get; }
}
