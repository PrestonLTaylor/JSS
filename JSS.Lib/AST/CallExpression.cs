namespace JSS.Lib.AST;

// 13.3.6 Function Calls, https://tc39.es/ecma262/#sec-function-calls
internal sealed class CallExpression : IExpression
{
    public CallExpression(IExpression lhs, List<IExpression> arguments)
    {
        Lhs = lhs;
        Arguments = arguments;
    }

    // FIXME: 13.3.6.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-calls-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public IReadOnlyList<IExpression> Arguments { get; }
}
