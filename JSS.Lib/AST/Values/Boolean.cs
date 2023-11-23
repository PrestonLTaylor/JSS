namespace JSS.Lib.AST.Values;

internal sealed class Boolean : Value
{
    public Boolean(bool value)
    {
        Value = value;
    }

    public override bool IsBoolean() { return true; }

    public bool Value { get; }
}
