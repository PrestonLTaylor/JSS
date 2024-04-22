using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

internal sealed class ArrayLiteral : IExpression
{
    // 13.2.4.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-array-initializer-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // FIXME: Implement the rest of the evaluation when we parse element lists
        // 1. Let array be ! ArrayCreate(0).
        var array = MUST(Array.ArrayCreate(0));

        // 3. Return array.
        return array;
    }
}
