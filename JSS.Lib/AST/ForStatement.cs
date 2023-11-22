namespace JSS.Lib.AST;

// 14.7.4 The for Statement, https://tc39.es/ecma262/#sec-for-statement
internal sealed class ForStatement : INode
{
    public ForStatement(INode? initializationExpression, INode? testExpression, INode? incrementExpression, INode iterationStatement)
    {
        InitializationExpression = initializationExpression;
        TestExpression = testExpression;
        IncrementExpression = incrementExpression;
        IterationStatement = iterationStatement;
    }

    // FIXME: 14.7.4.2 Runtime Semantics: ForLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-forloopevaluation
    public void Execute() { }

    // FIXME: This is kind of a misnomer, as we can have (non-)lexical declarations
    public INode? InitializationExpression { get; }
    public INode? TestExpression { get; }
    public INode? IncrementExpression { get; }
    public INode IterationStatement { get; }
}
