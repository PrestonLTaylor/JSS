namespace JSS.Lib.AST.Values;

internal sealed class Boolean : Value
{
    public Boolean(bool value)
    {
        Value = value;
    }

    public static implicit operator Boolean(bool value) => new(value);
    public static implicit operator bool(Boolean boolean) => boolean.Value;

    override public bool IsBoolean() { return true; }
    override public ValueType Type() { return ValueType.Boolean; }

    override public bool Equals(object? obj)
    {
        if (obj is not Boolean rhs) return false;
        return Value.Equals(rhs.Value);
    }

    override public int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Value { get; }
}
