﻿using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
internal sealed class ConstDeclaration : Declaration
{
    public ConstDeclaration(string identifier, INode initializer)
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

    // 14.3.1.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-let-and-const-declarations-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Let bindingId be StringValue of BindingIdentifier.
        // 2. Let lhs be ! ResolveBinding(bindingId).
        var lhs = MUST(ScriptExecutionContext.ResolveBinding(vm, Identifier));

        // FIXME: 3. If IsAnonymousFunctionDefinition(Initializer) is true, then
        // FIXME: a. Let value be ? NamedEvaluation of Initializer with argument bindingId.
        // FIXME: 4. Else,

        // a. Let rhs be ? Evaluation of Initializer.
        var rhs = Initializer!.Evaluate(vm);
        if (rhs.IsAbruptCompletion()) return rhs;

        // b. Let value be ? GetValue(rhs).
        var value = rhs.Value.GetValue(vm);
        if (value.IsAbruptCompletion()) return value;

        // 5. Perform ! InitializeReferencedBinding(lhs, value).
        var asReference = lhs.AsReference();
        MUST(asReference.InitializeReferencedBinding(vm, value.Value));

        // 6. Return EMPTY.
        return Empty.The;
    }

    public string Identifier { get; }

    // FIXME: Maybe have a more granular class for "AssignmentExpression"s: https://tc39.es/ecma262/#prod-AssignmentExpression
    public INode Initializer { get; }
}
