namespace JSS.Lib.AST.Literal;

// FIXME: This interface seems a bit clunky
internal sealed class BooleanLiteral : IExpression
{
    public BooleanLiteral(bool value)
    {
        _value = new Value.Boolean { Value = value };
    }

    public void Execute() { }

    public bool Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Value.Boolean _value;
}
