namespace JSS.Lib.AST;

// 14.5 Expression Statement, https://tc39.es/ecma262/#sec-expression-statement
internal sealed class ExpressionStatement : INode
{
    public ExpressionStatement(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 14.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-expression-statement-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
