namespace JSS.Lib.AST.Values;

internal sealed class String : Value
{
    public String(string value)
    {
        Value = value;
    }

    public static implicit operator String(string value) => new(value);
    public static implicit operator string(String @string) => @string.Value;

    override public bool IsString() { return true; }
    override public ValueType Type() { return ValueType.String; }

    override public bool Equals(object? obj)
    {
        if (obj is not String rhs) return false;
        return Value.Equals(rhs.Value);
    }

    override public int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public string Value { get; }
}
