using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

internal sealed record Binding(Value Value, bool Mutable)
{
}
