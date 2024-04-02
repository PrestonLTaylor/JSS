using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.2.1 The this Keyword, https://tc39.es/ecma262/#sec-this-keyword
internal sealed class ThisExpression : IExpression
{
    // 13.2.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-this-keyword-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? ResolveThisBinding().
        return ScriptExecutionContext.ResolveThisBinding(vm);
    }
}
