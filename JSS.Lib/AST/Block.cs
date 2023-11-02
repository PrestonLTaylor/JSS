namespace JSS.Lib.AST;

// 14.2 Block, https://tc39.es/ecma262/#sec-block
internal sealed class Block : INode 
{
    public Block(List<INode> nodes)
    {
        Nodes = nodes;
    }

    // FIXME: 14.2.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    public void Execute() { }

    public IReadOnlyList<INode> Nodes { get; }
}
