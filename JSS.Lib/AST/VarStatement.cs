using JSS.Lib.Execution;
using ExecutionContext = JSS.Lib.Execution.ExecutionContext;

namespace JSS.Lib.AST;

// 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
internal sealed class VarStatement : INode
{
    public VarStatement(string identifier, INode? initializer)
    {
        Identifier = identifier;
        Initializer = initializer;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return the BoundNames of BindingIdentifier.
        return new List<string> { Identifier };
    }

    // 14.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-variable-statement-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (Initializer is null)
        {
            return EvaluateWithoutInitializer(vm);
        }
        else
        {
            return EvaluateWithInitializer(vm);
        }
    }

    private Completion EvaluateWithoutInitializer(VM vm)
    {
        // 1. Return EMPTY.
        return Completion.NormalCompletion(vm.Empty);
    }

    private Completion EvaluateWithInitializer(VM vm)
    {
        // 1. Let bindingId be StringValue of BindingIdentifier.
        var bindingId = Identifier;

        // 2. Let lhs be ? ResolveBinding(bindingId).
        var lhs = ScriptExecutionContext.ResolveBinding(vm, bindingId);
        if (lhs.IsAbruptCompletion()) return lhs;

        // FIXME: 3. If IsAnonymousFunctionDefinition(Initializer) is true, then
        // FIXME: a. Let value be ? NamedEvaluation of Initializer with argument bindingId.

        // 4. Else,
        // a. Let rhs be ? Evaluation of Initializer.
        var rhs = Initializer!.Evaluate(vm);
        if (rhs.IsAbruptCompletion()) return rhs;

        // b. Let value be ? GetValue(rhs).
        var value = rhs.Value.GetValue();
        if (value.IsAbruptCompletion()) return value;

        // 5. Perform ? PutValue(lhs, value).
        var putResult = lhs.Value.PutValue(vm, value.Value);
        if (putResult.IsAbruptCompletion()) return putResult;

        // 6. Return EMPTY.
        return Completion.NormalCompletion(vm.Empty);
    }

    public string Identifier { get; }

    public INode? Initializer { get; }
}
