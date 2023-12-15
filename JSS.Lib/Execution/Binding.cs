using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

internal record struct Binding(Value Value, bool Mutable, bool Strict)
{
}
