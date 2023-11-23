using JSS.Lib.AST.Value;

namespace JSS.Lib.AST.Literal;

// 13.2.3 Literals
internal sealed class NumericLiteral : IExpression
{
    public NumericLiteral(double value)
    {
        _value = new Number { Value = value };
    }

    // FIXME: 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation

    public double Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Number _value;
}
