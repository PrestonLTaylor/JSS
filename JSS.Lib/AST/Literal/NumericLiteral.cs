using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

// 13.2.3 Literals
internal sealed class NumericLiteral : IExpression
{
    public NumericLiteral(double value)
    {
        _value = new Number(value);
    }

    // 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation
    override public Completion Evaluate(VM _)
    {
        // 1. Return the NumericValue of NumericLiteral as defined in 12.9.3.
        return _value;
    }

    public double Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Number _value;
}
