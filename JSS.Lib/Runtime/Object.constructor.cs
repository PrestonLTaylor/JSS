﻿using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.1.2 Properties of the Object Constructor, https://tc39.es/ecma262/#sec-object-value
internal class ObjectConstructor : Object, ICallable, IConstructable
{
    // The Object constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public ObjectConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm, VM vm)
    {
        // The Object constructor has a "length" property whose value is 1𝔽.
        // FIXME: We should probably have a method for internally defining properties
        InternalDefineProperty("length", 1, new(true, false, true));

        // 20.1.2.4 Object.defineProperty ( O, P, Attributes ), https://tc39.es/ecma262/#sec-object.defineproperty
        InternalDefineProperty(vm, "defineProperty", 3, defineProperty, new(true, false, true));

        // 20.1.2.21 Object.prototype, The initial value of Object.prototype is the Object prototype object.
        // This property has the attributes { [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: false }.
        InternalDefineProperty("prototype", realm.ObjectPrototype, new(false, false, false));

        // 20.1.2.8 Object.getOwnPropertyDescriptor ( O, P ), https://tc39.es/ecma262/#sec-object.getownpropertydescriptor
        InternalDefineProperty(vm, "getOwnPropertyDescriptor", 2, getOwnPropertyDescriptor, new(true, false, true));

        // 20.1.2.10 Object.getOwnPropertyNames ( O ), https://tc39.es/ecma262/#sec-object.getownpropertynames
        InternalDefineProperty(vm, "getOwnPropertyNames", 1, getOwnPropertyNames, new(true, false, true));
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The); 
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value 
    public Completion Construct(VM vm, List argumentList, Object newTarget)
    {
        // FIXME: 1. If NewTarget is neither undefined nor the active function object, then
        // FIXME: a. Return ? OrdinaryCreateFromConstructor(NewTarget, "%Object.prototype%").

        // 2. If value is either undefined or null, return OrdinaryObjectCreate(%Object.prototype%).
        var value = argumentList[0];
        if (value.IsUndefined() || value.IsNull())
        {
            return new Object(vm.ObjectPrototype);
        }

        // 3. Return ! ToObject(value).
        return MUST(value.ToObject(vm));
    }

    // 20.1.2.4 Object.defineProperty ( O, P, Attributes ), https://tc39.es/ecma262/#sec-object.defineproperty
    private Completion defineProperty(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. If O is not an Object, throw a TypeError exception.
        var O = argumentList[0];
        if (!O.IsObject()) return ThrowTypeError(vm, RuntimeErrorType.TriedToDefinePropertyOnNonObject, O.Type());

        // 2. Let key be ? ToPropertyKey(P).
        var key = argumentList[1].ToPropertyKey(vm);
        if (key.IsAbruptCompletion()) return key;

        // 3. Let desc be ? ToPropertyDescriptor(Attributes).
        var desc = argumentList[2].ToPropertyDescriptor(vm);

        // 4. Perform ? DefinePropertyOrThrow(O, key, desc).
        var defineResult = DefinePropertyOrThrow(vm, O.AsObject(), key.Value.AsString(), desc.Value);
        if (defineResult.IsAbruptCompletion()) return defineResult;

        // 5. Return O.
        return O;
    }

    // 20.1.2.8 Object.getOwnPropertyDescriptor ( O, P ), https://tc39.es/ecma262/#sec-object.getownpropertydescriptor
    private Completion getOwnPropertyDescriptor(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Let obj be ? ToObject(O).
        var obj = argumentList[0].ToObject(vm);
        if (obj.IsAbruptCompletion()) return obj.Completion;

        // 2. Let key be ? ToPropertyKey(P).
        var key = argumentList[1].ToPropertyKey(vm);
        if (key.IsAbruptCompletion()) return key;

        // 3. Let desc be ? obj.[[GetOwnProperty]](key).
        // FIXME: When we implement symbols, we can't rely on AsString here
        var desc = obj.Value.GetOwnProperty(key.Value.AsString());
        if (desc.IsAbruptCompletion()) return desc;

        // 4. Return FromPropertyDescriptor(desc).
        return desc.Value.FromPropertyDescriptor(vm);
    }

    // 20.1.2.10 Object.getOwnPropertyNames ( O ), https://tc39.es/ecma262/#sec-object.getownpropertynames
    private Completion getOwnPropertyNames(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Return CreateArrayFromList(? GetOwnPropertyKeys(O, STRING)).
        var ownPropertyKeys = GetOwnPropertyKeys(vm, argumentList[0]);
        if (ownPropertyKeys.IsAbruptCompletion()) return ownPropertyKeys.Completion;
        return ownPropertyKeys.Value.CreateArrayFromList(vm);
    }

    // 20.1.2.11.1 GetOwnPropertyKeys ( O, FIXME: type ), https://tc39.es/ecma262/#sec-getownpropertykeys
    private AbruptOr<List> GetOwnPropertyKeys(VM vm, Value O)
    {
        // 1. Let obj be ? ToObject(O).
        var obj = O.ToObject(vm);
        if (obj.IsAbruptCompletion()) return obj.Completion;

        // 2. Let keys be ? obj.[[OwnPropertyKeys]]().
        var keys = obj.Value.OwnPropertyKeys();
        if (keys.IsAbruptCompletion()) return keys;

        // 3. Let nameList be a new empty List.
        var nameList = new List();

        // 4. For each element nextKey of keys, do
        foreach (var nextKey in keys.Value.Values)
        {
            // FIXME: a. If nextKey is a Symbol and type is SYMBOL, or if nextKey is a String and type is STRING, then
            // i. Append nextKey to nameList.
            nameList.Add(nextKey);
        }

        // 5. Return nameList.
        return nameList;
    }
}
