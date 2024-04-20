using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

internal class Binding
{
    public Binding(Value value, bool mutable, bool strict)
    {
        Value = value;
        Mutable = mutable;
        Strict = strict;
    }

    public Value Value { get; set; }
    public bool Mutable { get; }
    public bool Strict { get; }
}
