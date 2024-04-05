using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

// 13.2.3 Literals
internal sealed class StringLiteral : IExpression
{
    public StringLiteral(string value)
    {
        _value = value;
    }

    // 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation
    override public Completion Evaluate(VM _)
    {
        // 1. Return the SV of StringLiteral as defined in 12.9.4.2.
        return _value;
    }

    public string Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly String _value;
}
