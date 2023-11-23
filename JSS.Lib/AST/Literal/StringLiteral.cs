namespace JSS.Lib.AST.Literal;

// 13.2.3 Literals
internal sealed class StringLiteral : IExpression
{
    public StringLiteral(string value)
    {
        _value = new Value.String(value);
    }

    // FIXME: 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation

    public string Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Value.String _value;
}
