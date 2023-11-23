namespace JSS.Lib.AST.Values;

internal sealed class String : Value
{
    public String(string value)
    {
        Value = value;
    }

    public override bool IsString() { return true; }

    public string Value { get; }
}
