using JSS.Lib.AST.Values;
using System.Diagnostics;
using String = JSS.Lib.AST.Values.String;

namespace JSS.Lib.Execution;

// 9.1.1.1 Declarative Environment Records, https://tc39.es/ecma262/#sec-declarative-environment-records
internal sealed class DeclarativeEnvironment : Environment
{
    // 9.1.2.2 NewDeclarativeEnvironment ( E ), https://tc39.es/ecma262/#sec-newdeclarativeenvironment
    public DeclarativeEnvironment(Environment? outerEnv)
    {
        // 1. Let env be a new Declarative Environment Record containing no bindings.

        // 2. Set env.[[OuterEnv]] to E.
        OuterEnv = outerEnv;

        // 3. Return env.
    }

    // 9.1.1.1.1 HasBinding ( N ), https://tc39.es/ecma262/#sec-declarative-environment-records
    override public bool HasBinding(string N)
    {
        // 1. If envRec has a binding for N, return true.
        // 2. Return false.
        return _identifierToBinding.ContainsKey(N);
    }

    // 9.1.1.1.5 SetMutableBinding ( N, V, S ), https://tc39.es/ecma262/#sec-declarative-environment-records-getbindingvalue-n-s
    public override Completion SetMutableBinding(VM vm, string N, Value V, bool S)
    {
        // 1. If envRec does not have a binding for N, then
        if (!_identifierToBinding.ContainsKey(N))
        {
            // a. If S is true, throw a ReferenceError exception.
            if (S)
            {
                return Completion.ThrowCompletion(new String($"{N} is not defined."));
            }

            // FIXME: b. Perform ! envRec.CreateMutableBinding(N, true).
            // FIXME: c. Perform ! envRec.InitializeBinding(N, V).

            // d. Return UNUSED.
            return Completion.NormalCompletion(vm.Empty);
        }

        // FIXME: 2. If the binding for N in envRec is a strict binding, set S to true.

        // FIXME: 3. If the binding for N in envRec has not yet been initialized, then
        // FIXME: a. Throw a ReferenceError exception.

        // 4. Else if the binding for N in envRec is a mutable binding, then
        // a. Change its bound value to V.
        var binding = _identifierToBinding[N];
        binding.Value = V;

        // FIXME: 5. Else,
        // FIXME: a. Assert: This is an attempt to change the value of an immutable binding.
        // FIXME: b. If S is true, throw a TypeError exception.

        // 6. Return UNUSED.
        return Completion.NormalCompletion(vm.Empty);
    }

    // 9.1.1.1.6 GetBindingValue ( N, S ), https://tc39.es/ecma262/#sec-declarative-environment-records-getbindingvalue-n-s
    override public Completion GetBindingValue(string N, bool S)
    {
        // 1. Assert: envRec has a binding for N.
        Debug.Assert(_identifierToBinding.ContainsKey(N));

        // FIXME: 2. If the binding for N in envRec is an uninitialized binding, throw a ReferenceError exception.

        // 3. Return the value currently bound to N in envRec.
        var binding = _identifierToBinding[N];
        return Completion.NormalCompletion(binding.Value);
    }

    private readonly Dictionary<string, Binding> _identifierToBinding = new();
}
