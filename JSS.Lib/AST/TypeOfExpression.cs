namespace JSS.Lib.AST;

// 13.5.3 The typeof Operator, https://tc39.es/ecma262/#sec-typeof-operator
internal sealed class TypeOfExpression : IExpression
{
    public TypeOfExpression(IExpression expression)
    {
        Expression = expression;
    }

    // FIXME: 13.5.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-typeof-operator-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Expression { get; }
}
