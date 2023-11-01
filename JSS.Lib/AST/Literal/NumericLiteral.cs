using JSS.Lib.AST.Value;

namespace JSS.Lib.AST.Literal;

internal sealed class NumericLiteral : IExpression
{
    public NumericLiteral(double value)
    {
        _value = new Number { Value = value };
    }

    public void Execute() { }

    public double Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Number _value;
}
