namespace JSS.Lib.AST.Literal;

// FIXME: This interface seems a bit clunky
// 13.2.3 Literals
internal sealed class BooleanLiteral : IExpression
{
    public BooleanLiteral(bool value)
    {
        _value = new Value.Boolean { Value = value };
    }

    // FIXME: 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation

    public bool Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Value.Boolean _value;
}
