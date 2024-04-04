using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;

namespace JSS.Lib.AST.Literal;

// FIXME: This interface seems a bit clunky
// 13.2.3 Literals
internal sealed class BooleanLiteral : IExpression
{
    public BooleanLiteral(bool value)
    {
        _value = value;
    }

    // 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation
    override public Completion Evaluate(VM _)
    {
        // 1. If BooleanLiteral is the token false, return false.
        // 2. If BooleanLiteral is the token true, return true.
        return _value;
    }

    public bool Value
    {
        get
        {
            return _value.Value;
        }
    }

    private readonly Boolean _value;
}
