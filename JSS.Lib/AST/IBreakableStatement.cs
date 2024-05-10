using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal interface IBreakableStatement
{
    public Completion EvaluateFromLabelled(VM vm);
}
