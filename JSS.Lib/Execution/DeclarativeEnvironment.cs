using JSS.Lib.AST.Values;
using System.Diagnostics;

namespace JSS.Lib.Execution;

// 9.1.1.1 Declarative Environment Records, https://tc39.es/ecma262/#sec-declarative-environment-records
internal class DeclarativeEnvironment : Environment
{
    // 9.1.2.2 NewDeclarativeEnvironment ( E ), https://tc39.es/ecma262/#sec-newdeclarativeenvironment
    public DeclarativeEnvironment(Environment? outerEnv)
    {
        // 1. Let env be a new Declarative Environment Record containing no bindings.

        // 2. Set env.[[OuterEnv]] to E.
        OuterEnv = outerEnv;

        // 3. Return env.
    }

    // 9.1.1.1.1 HasBinding ( N ), https://tc39.es/ecma262/#sec-declarative-environment-records-hasbinding-n
    override public bool HasBinding(string N)
    {
        // 1. If envRec has a binding for N, return true.
        // 2. Return false.
        return _identifierToBinding.ContainsKey(N);
    }

    // 9.1.1.1.2 CreateMutableBinding ( N, D ), https://tc39.es/ecma262/#sec-declarative-environment-records-createmutablebinding-n-d
    override public Completion CreateMutableBinding(VM _, string N, bool D)
    {
        // 1. Assert: envRec does not already have a binding for N.
        Debug.Assert(!_identifierToBinding.ContainsKey(N));

        // 2. Create a mutable binding in envRec for N and record that it is FIXME: uninitialized.
        // FIXME: If D is true, record that the newly created binding may be deleted by a subsequent DeleteBinding call.
        _identifierToBinding.Add(N, new Binding(Undefined.The, true, false));

        // 3. Return unused.
        return Empty.The;
    }

    // 9.1.1.1.3 CreateImmutableBinding ( N, S ), https://tc39.es/ecma262/#sec-declarative-environment-records-createimmutablebinding-n-s
    override public Completion CreateImmutableBinding(VM _, string N, bool S)
    {
        // 1. Assert: envRec does not already have a binding for N.
        Debug.Assert(!_identifierToBinding.ContainsKey(N));

        // 2. Create an immutable binding in envRec for N and record that it is FIXME: uninitialized.
        // If S is true, record that the newly created binding is a strict binding.
        _identifierToBinding.Add(N, new Binding(Undefined.The, false, S));

        // 3. Return unused.
        return Empty.The;
    }

    // 9.1.1.1.4 InitializeBinding ( N, V ), https://tc39.es/ecma262/#sec-declarative-environment-records-initializebinding-n-v
    public override Completion InitializeBinding(VM _, string N, Value V)
    {
        // FIXME: 1. Assert: envRec must have an uninitialized binding for N.

        // 2. Set the bound value for N in envRec to V.
        var binding = _identifierToBinding[N];
        binding.Value = V;
        _identifierToBinding[N] = binding;

        // FIXME: 3. Record that the binding for N in envRec has been initialized.

        // 4. Return unused.
        return Empty.The;
    }

    // 9.1.1.1.5 SetMutableBinding ( N, V, S ), https://tc39.es/ecma262/#sec-declarative-environment-records-setmutablebinding-n-v-s
    public override Completion SetMutableBinding(VM vm, string N, Value V, bool S)
    {
        // 1. If envRec does not have a binding for N, then
        if (!_identifierToBinding.ContainsKey(N))
        {
            // a. If S is true, throw a ReferenceError exception.
            if (S)
            {
                return ThrowReferenceError(vm, RuntimeErrorType.BindingNotDefined, N);
            }

            // b. Perform ! envRec.CreateMutableBinding(N, true).
            MUST(CreateMutableBinding(vm, N, true));

            // c. Perform ! envRec.InitializeBinding(N, V).
            MUST(InitializeBinding(vm, N, V));

            // d. Return UNUSED.
            return Empty.The;
        }

        // 2. If the binding for N in envRec is a strict binding, set S to true.
        var binding = _identifierToBinding[N];
        if (binding.Strict)
        {
            S = true;
        }

        // FIXME: 3. If the binding for N in envRec has not yet been initialized, then
        // FIXME: a. Throw a ReferenceError exception.

        // 4. Else if the binding for N in envRec is a mutable binding, then
        if (binding.Mutable)
        {
            // a. Change its bound value to V.
            binding.Value = V;
        }
        // 5. Else,
        else
        {
            // NOTE: This assert is intrinsic
            // a. Assert: This is an attempt to change the value of an immutable binding.

            // b. If S is true, throw a TypeError exception.
            if (S) return ThrowTypeError(vm, RuntimeErrorType.AssignmentToConst, N);
        }

        // 6. Return UNUSED.
        return Empty.The;
    }

    // 9.1.1.1.6 GetBindingValue ( N, S ), https://tc39.es/ecma262/#sec-declarative-environment-records-getbindingvalue-n-s
    override public Completion GetBindingValue(VM _, string N, bool S)
    {
        // 1. Assert: envRec has a binding for N.
        Debug.Assert(_identifierToBinding.ContainsKey(N));

        // FIXME: 2. If the binding for N in envRec is an uninitialized binding, throw a ReferenceError exception.

        // 3. Return the value currently bound to N in envRec.
        var binding = _identifierToBinding[N];
        return binding.Value;
    }

    // 9.1.1.1.7 DeleteBinding ( N ), https://tc39.es/ecma262/#sec-declarative-environment-records-deletebinding-n
    public override Completion DeleteBinding(string N)
    {
        // 1. Assert: envRec has a binding for N.
        Debug.Assert(_identifierToBinding.ContainsKey(N));

        // 2. If the binding for N in envRec cannot be deleted, return false.

        // 3. Remove the binding for N from envRec.
        _identifierToBinding.Remove(N);

        // 4. Return true.
        return true;
    }

    // 9.1.1.1.8 HasThisBinding ( ), https://tc39.es/ecma262/#sec-declarative-environment-records-hasthisbinding
    override public bool HasThisBinding()
    {
        // 1. Return false.
        return false;
    }

    // 9.1.1.1.10 WithBaseObject ( ), https://tc39.es/ecma262/#sec-declarative-environment-records-withbaseobject
    public override Value WithBaseObject()
    {
        // 1. Return undefined.
        return Undefined.The;
    }

    private readonly Dictionary<string, Binding> _identifierToBinding = new();
}
