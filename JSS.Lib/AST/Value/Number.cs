namespace JSS.Lib.AST.Value;

internal sealed class Number : Value
{
    public override bool IsNumber() { return true; }

    public double Value { get; init; }
}
