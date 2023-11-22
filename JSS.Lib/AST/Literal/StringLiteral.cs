namespace JSS.Lib.AST.Literal;

internal sealed class StringLiteral : IExpression
{
    public StringLiteral(string value)
    {
        _value = new Value.String(value);
    }

    public void Execute() { }

    public string Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Value.String _value;
}
