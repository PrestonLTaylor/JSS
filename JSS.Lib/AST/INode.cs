using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal abstract class INode
{
    virtual public Completion Evaluate(VM vm) { throw new NotImplementedException(); }
}
