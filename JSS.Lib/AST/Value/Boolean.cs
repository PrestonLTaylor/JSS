namespace JSS.Lib.AST.Value;

internal sealed class Boolean : Value
{
    public override bool IsBoolean() { return true; }

    public bool Value { get; init; }
}
