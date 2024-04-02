using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

internal interface ICallable
{
    public Completion Call(VM vm, Value thisArgument, List argumentList);
}
