namespace JSS.Lib.AST;

internal sealed class Program
{
    public Program(List<INode> rootNodes)
    {
        RootNodes = rootNodes;
    }

    public List<INode> RootNodes { get; }
}
