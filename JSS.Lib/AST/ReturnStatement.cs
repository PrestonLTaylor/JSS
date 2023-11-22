namespace JSS.Lib.AST;

// 14.10 The return Statement, https://tc39.es/ecma262/#sec-return-statement
internal sealed class ReturnStatement : INode
{
    public ReturnStatement(IExpression? returnExpression)
    {
        ReturnExpression = returnExpression;
    }

    // FIXME: 14.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-return-statement-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression? ReturnExpression { get; }
}
