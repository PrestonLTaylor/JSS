namespace JSS.Lib.AST;

// 13.3.7 The super Keyword, https://tc39.es/ecma262/#sec-super-keyword
internal sealed class SuperComputedPropertyExpression : IExpression
{
    public SuperComputedPropertyExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.3.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-super-keyword-runtime-semantics-evaluation

    public IExpression Expression { get; }
}
