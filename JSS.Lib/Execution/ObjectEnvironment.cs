﻿using Boolean = JSS.Lib.AST.Values.Boolean;
using Object = JSS.Lib.AST.Values.Object;
using String = JSS.Lib.AST.Values.String;

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
        var foundBinding = (bindingObject.HasProperty(N).Value as Boolean)!;

        // 3. If foundBinding is false, return false.
        return foundBinding.Value;

        // FIXME: 4. If envRec.[[IsWithEnvironment]] is false, return true.
        // FIXME: 5. Let unscopables be ? Get(bindingObject, @@unscopables).
        // FIXME: 6. If unscopables is an Object, then
        // FIXME: a. Let blocked be ToBoolean(? Get(unscopables, N)).
        // FIXME: b. If blocked is true, return false.
        // FIXME: 7. Return true.
    }

    // 9.1.1.2.6 GetBindingValue ( N, S ), https://tc39.es/ecma262/#sec-object-environment-records-getbindingvalue-n-s
    override public Completion GetBindingValue(string N, bool S)
    {
        // 1. Let bindingObject be envRec.[[BindingObject]].
        // 2. Let value be ? HasProperty(bindingObject, N).
        var value = BindingObject.HasProperty(N);
        if (value.IsAbruptCompletion()) return value;

        // 3. If value is false, then
        var asBoolean = (value.Value as Boolean)!;
        if (!asBoolean.Value)
        {
            // FIXME: a. If S is false, return undefined; otherwise throw a ReferenceError exception.
            return Completion.ThrowCompletion(new String($"{N} is not defined."));
        }

        // 4. Return ? Get(bindingObject, N).
        return Object.Get(BindingObject, N);
    }

    public Object BindingObject { get; }
    public bool IsWithEnvironment { get; }
}