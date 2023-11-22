namespace JSS.Lib.AST.Value;

internal sealed class String : Value
{
    public String(string value)
    {
        Value = value;
    }

    public override bool IsString() { return true; }

    public string Value { get; }
}
