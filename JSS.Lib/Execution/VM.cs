using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

// NOTE: This isn't required in the spec but is a helper for executing code
internal sealed class VM
{
    public Value Empty { get; } = new Empty();
    public Value Null { get; } = new Null();
    public Value Undefined { get; } = new Undefined();
    public Value NaN { get; } = new Number(double.NaN);
}
