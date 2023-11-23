namespace JSS.Lib.AST.Value;

internal sealed class Number : Value
{
    public Number(double value)
    {
        Value = value;
    }

    public override bool IsNumber() { return true; }

    public double Value { get; init; }
}
