namespace JSS.Lib.AST;

// 14.15 The try Statement, https://tc39.es/ecma262/#sec-try-statement
internal sealed class TryStatement : INode
{
    public TryStatement(Block tryBlock, Block? catchBlock, Identifier? catchParameter, Block? finallyBlock)
    {
        TryBlock = tryBlock;
        CatchBlock = catchBlock;
        CatchParameter = catchParameter;
        FinallyBlock = finallyBlock;
    }

    // 14.15.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-try-statement-runtime-semantics-evaluation

    public Block TryBlock { get; }
    public Block? CatchBlock { get; }
    public Identifier? CatchParameter { get; }
    public Block? FinallyBlock { get; }
}
