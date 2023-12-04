using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.1 Identifiers, https://tc39.es/ecma262/#sec-identifiers
internal sealed class Identifier : IExpression
{
    public Identifier(string name)
    {
        Name = name;
    }

    // 13.1.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-identifiers-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? ResolveBinding(StringValue of Identifier).
        return ScriptExecutionContext.ResolveBinding(vm, Name);
    }

    public string Name { get; }
}
