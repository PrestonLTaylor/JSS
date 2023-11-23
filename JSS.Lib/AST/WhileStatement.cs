namespace JSS.Lib.AST;

// 14.7.3 The while Statement, https://tc39.es/ecma262/#sec-while-statement
internal sealed class WhileStatement : INode
{
    public WhileStatement(IExpression whileExpression, INode iterationStatement)
    {
        WhileExpression = whileExpression;
        IterationStatement = iterationStatement;
    }

    // FIXME: 14.7.3.2 Runtime Semantics: WhileLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-whileloopevaluation

    public IExpression WhileExpression { get; }
    public INode IterationStatement { get; }
}
