namespace JSS.Lib.AST.Values;

internal sealed class Number : Value
{
    public Number(double value)
    {
        Value = value;
    }

    public override bool IsNumber() { return true; }

    override public bool Equals(object? obj)
    {
        if (obj is not Number rhs) return false;
        return Value.Equals(rhs.Value);
    }

    override public int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public double Value { get; init; }
}
