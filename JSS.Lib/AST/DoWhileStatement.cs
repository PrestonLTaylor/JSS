namespace JSS.Lib.AST;

// 14.7.2 The do-while Statement, https://tc39.es/ecma262/#sec-do-while-statement
internal sealed class DoWhileStatement : INode
{
    public DoWhileStatement(IExpression whileExpression, INode iterationStatement)
    {
        WhileExpression = whileExpression;
        IterationStatement = iterationStatement;
    }

    // FIXME: 14.7.2.2 Runtime Semantics: DoWhileLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-dowhileloopevaluation

    public IExpression WhileExpression { get; }
    public INode IterationStatement { get; }
}
