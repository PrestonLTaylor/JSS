using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

internal abstract class Callable : Object
{
    public Callable(Object? prototype) : base(prototype) { }

    override public bool HasInternalCall() { return true; }

    abstract public Completion Call(VM vm, Value thisArgument, List argumentList);
}
