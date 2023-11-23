namespace JSS.Lib.AST;

internal readonly struct CaseBlock
{
    public CaseBlock(IExpression caseExpression, IReadOnlyList<INode> statementList)
    {
        CaseExpression = caseExpression;
        StatementList = statementList;
    }

    public IExpression CaseExpression { get; }
    public IReadOnlyList<INode> StatementList { get; }
}

internal readonly struct DefaultBlock
{
    public DefaultBlock(IReadOnlyList<INode> statementList)
    {
        StatementList = statementList;
    }

    public IReadOnlyList<INode> StatementList { get; }
}

// 14.12 The switch Statement, https://tc39.es/ecma262/#sec-switch-statement
internal sealed class SwitchStatement : INode
{
    public SwitchStatement(IExpression switchExpression, List<CaseBlock> caseBlocks, DefaultBlock? defaultCase)
    {
        SwitchExpression = switchExpression;
        CaseBlocks = caseBlocks;
        DefaultCase = defaultCase;
    }

    // FIXME: 14.12.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-switch-statement-runtime-semantics-evaluation

    public IExpression SwitchExpression { get; }
    public IReadOnlyList<CaseBlock> CaseBlocks { get; }
    public DefaultBlock? DefaultCase { get; }
}
