namespace JSS.Lib.AST;

// 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
internal sealed class IfStatement : INode
{
    public IfStatement(IExpression ifExpression, INode ifCaseStatement, INode? elseCaseStatement)
    {
        IfExpression = ifExpression;
        IfCaseStatement = ifCaseStatement;
        ElseCaseStatement = elseCaseStatement;
    }

    // FIXME: 14.6.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-if-statement-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression IfExpression { get; }
    public INode IfCaseStatement { get; }
    public INode? ElseCaseStatement { get; }
}
