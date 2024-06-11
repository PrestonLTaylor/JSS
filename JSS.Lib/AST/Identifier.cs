using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.1 Identifiers, https://tc39.es/ecma262/#sec-identifiers
internal sealed class Identifier : IExpression
{
    public Identifier(string name)
    {
        Name = name;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return a List whose sole element is the StringValue of Identifier.
        return new List<string> { Name };
    }

    // 8.6.2 Runtime Semantics: BindingInitialization, https://tc39.es/ecma262/#sec-runtime-semantics-bindinginitialization
    public Completion BindingInitialization(VM vm, Value value, Value environment)
    {
        // 1. Let name be StringValue of Identifier.
        // 2. Return ? InitializeBoundName(name, value, environment).
        return ScriptExecutionContext.InitializeBoundName(vm, Name, value, environment);
    }

    // 13.1.3 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-identifiers-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return ? ResolveBinding(StringValue of Identifier).
        return ScriptExecutionContext.ResolveBinding(vm, Name);
    }

    public string Name { get; }
}
