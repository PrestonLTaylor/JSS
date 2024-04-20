using JSS.Lib.AST.Values;
namespace JSS.Lib.Execution;

internal sealed class ObjectEnvironment : Environment
{
    // 9.1.2.3 NewObjectEnvironment ( O, W, E ), https://tc39.es/ecma262/#sec-newobjectenvironment
    public ObjectEnvironment(Object O, bool W, Environment? E)
    {
        // 1. Let env be a new Object Environment Record.

        // 2. Set env.[[BindingObject]] to O.
        BindingObject = O;

        // 3. Set env.[[IsWithEnvironment]] to W.
        IsWithEnvironment = W;

        // 4. Set env.[[OuterEnv]] to E.
        OuterEnv = E;

        // 5. Return env.
    }

    // 9.1.1.2.1 HasBinding ( N ), https://tc39.es/ecma262/#sec-object-environment-records-hasbinding-n
    override public bool HasBinding(string N)
    {
        // 1. Let bindingObject be envRec.[[BindingObject]].
        var bindingObject = BindingObject;

        // FIXME: Handle AbruptCompletions
        // 2. Let foundBinding be ? HasProperty(bindingObject, N).
        var foundBinding = Object.HasProperty(bindingObject, N).Value.AsBoolean();

        // 3. If foundBinding is false, return false.
        return foundBinding.Value;

        // FIXME: 4. If envRec.[[IsWithEnvironment]] is false, return true.
        // FIXME: 5. Let unscopables be ? Get(bindingObject, @@unscopables).
        // FIXME: 6. If unscopables is an Object, then
        // FIXME: a. Let blocked be ToBoolean(? Get(unscopables, N)).
        // FIXME: b. If blocked is true, return false.
        // FIXME: 7. Return true.
    }

    // 9.1.1.2.2 CreateMutableBinding ( N, D ), https://tc39.es/ecma262/#sec-object-environment-records-createmutablebinding-n-d
    override public Completion CreateMutableBinding(VM vm, string N, bool D)
    {
        // 1. Let bindingObject be envRec.[[BindingObject]].
        // 2. Perform ? DefinePropertyOrThrow(bindingObject, N, PropertyDescriptor { [[Value]]: undefined, [[Writable]]: true, [[Enumerable]]: true, [[Configurable]]: D }).
        var defineResult = Object.DefinePropertyOrThrow(vm, BindingObject, N, new Property(Undefined.The, new(true, true, D)));
        if (defineResult.IsAbruptCompletion()) return defineResult;

        // 3. Return UNUSED.
        return Empty.The;
    }

    // 9.1.1.2.4 InitializeBinding ( N, V ), https://tc39.es/ecma262/#sec-object-environment-records-initializebinding-n-v
    override public Completion InitializeBinding(VM vm, string N, Value V)
    {
        // 1. Perform ? envRec.SetMutableBinding(N, V, false).
        var setResult = SetMutableBinding(vm, N, V, false);
        if (setResult.IsAbruptCompletion()) return setResult;

        // 2. Return UNUSED.
        return Empty.The;
    }

    // 9.1.1.2.5 SetMutableBinding ( N, V, S ), https://tc39.es/ecma262/#sec-object-environment-records-getbindingvalue-n-s
    override public Completion SetMutableBinding(VM vm, string N, Value V, bool S)
    {
        // 1. Let bindingObject be envRec.[[BindingObject]].
        // 2. Let stillExists be ? HasProperty(bindingObject, N).
        var stillExists = Object.HasProperty(BindingObject, N);
        if (stillExists.IsAbruptCompletion()) return stillExists;

        // 3. If stillExists is false and S is true, throw a ReferenceError exception.
        var asBoolean = stillExists.Value.AsBoolean();
        if (!asBoolean.Value && S)
        {
            return ThrowReferenceError(vm, RuntimeErrorType.BindingNotDefined, N);
        }

        // 4. Perform ? Set(bindingObject, N, V, S).
        var setResult = Object.Set(vm, BindingObject, N, V, S);
        if (setResult.IsAbruptCompletion()) return setResult;

        // 5. Return UNUSED.
        return Empty.The;
    }

    // 9.1.1.2.6 GetBindingValue ( N, S ), https://tc39.es/ecma262/#sec-object-environment-records-getbindingvalue-n-s
    override public Completion GetBindingValue(VM vm, string N, bool S)
    {
        // 1. Let bindingObject be envRec.[[BindingObject]].
        // 2. Let value be ? HasProperty(bindingObject, N).
        var value = Object.HasProperty(BindingObject, N);
        if (value.IsAbruptCompletion()) return value;

        // 3. If value is false, then
        var asBoolean = value.Value.AsBoolean();
        if (!asBoolean.Value)
        {
            // a. If S is false, return undefined; otherwise throw a ReferenceError exception.
            if (S)
            {
                return ThrowReferenceError(vm, RuntimeErrorType.BindingNotDefined, N);
            }
            else
            {
                return Undefined.The;
            }
        }

        // 4. Return ? Get(bindingObject, N).
        return Object.Get(BindingObject, N);
    }

    // 9.1.1.2.8 HasThisBinding ( ), https://tc39.es/ecma262/#sec-object-environment-records-hasthisbinding
    override public bool HasThisBinding()
    {
        // 1. Return false.
        return false;
    }

    // 9.1.1.2.10 WithBaseObject ( ), https://tc39.es/ecma262/#sec-object-environment-records-withbaseobject
    public override Value WithBaseObject()
    {
        // 1. If envRec.[[IsWithEnvironment]] is true, return envRec.[[BindingObject]].
        if (IsWithEnvironment) return BindingObject;

        // 2. Otherwise, return undefined.
        return Undefined.The;
    }

    public Object BindingObject { get; }
    public bool IsWithEnvironment { get; }
}
