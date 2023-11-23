namespace JSS.Lib.AST;

// 14.14 The throw Statement, https://tc39.es/ecma262/#sec-throw-statement
internal sealed class ThrowStatement : INode
{
    public ThrowStatement(IExpression throwExpression)
    {
        ThrowExpression = throwExpression;
    }

    // FIXME: 14.14.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-throw-statement-runtime-semantics-evaluation

    public IExpression ThrowExpression { get; }
}
