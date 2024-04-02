using JSS.Lib.Execution;
using JSS.Lib.AST.Values;

namespace JSS.Lib.AST.Literal;

// 13.2.3 Literals
internal sealed class NullLiteral : IExpression
{
    // 13.2.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-literals-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return null.
        return Completion.NormalCompletion(Null.The);
    }
}
