using System.Diagnostics;

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
