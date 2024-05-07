using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.5.1 The delete Operator, https://tc39.es/ecma262/#sec-delete-operator
internal sealed class DeleteExpression : IExpression
{
    public DeleteExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.1.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-delete-operator-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let ref be ? Evaluation of UnaryExpression.
        var uref = Expression.Evaluate(vm);
        if (uref.IsAbruptCompletion()) return uref;

        // 2. If ref is not a Reference Record, return true.
        if (!uref.Value.IsReference()) return true;
        var asReference = uref.Value.AsReference();

        // 3. If IsUnresolvableReference(ref) is true, then
        if (asReference.IsUnresolvableReference())
        {
            // a. Assert: ref.[[Strict]] is false.
            Assert(!asReference.Strict, "a. Assert: ref.[[Strict]] is false.");

            // b. Return true.
            return true;
        }

        // 4. If IsPropertyReference(ref) is true, then
        if (asReference.IsPropertyReference())
        {
            // FIXME: a. Assert: IsPrivateReference(ref) is false.

            // FIXME: b. If IsSuperReference(ref) is true, throw a ReferenceError exception.

            // c. Let baseObj be ? ToObject(ref.[[Base]]).
            var baseObj = asReference.Base!.ToObject(vm);
            if (baseObj.IsAbruptCompletion()) return baseObj.Completion;

            // d. Let deleteStatus be ? baseObj.[[Delete]](ref.[[ReferencedName]]).
            var deleteStatus = baseObj.Value.Delete(asReference.ReferencedName);
            if (deleteStatus.IsAbruptCompletion()) return deleteStatus.Completion;

            // e. If deleteStatus is false and ref.[[Strict]] is true, throw a TypeError exception.
            if (!deleteStatus.Value && asReference.Strict) return ThrowTypeError(vm, RuntimeErrorType.FailedToDelete, asReference.ReferencedName);

            // f. Return deleteStatus.
            return deleteStatus.Value;
        }
        // 5. Else,
        else
        {
            // a. Let base be ref.[[Base]].
            var baseEnv = asReference.Base;

            // b. Assert: base is an Environment Record.
            Assert(baseEnv is Environment, "b. Assert: base is an Environment Record.");

            // c. Return ? base.DeleteBinding(ref.[[ReferencedName]]).
            var asEnvironment = baseEnv.AsEnvironment();
            return asEnvironment.DeleteBinding(asReference.ReferencedName);
        }
    }

    public IExpression Expression { get; }
}
