﻿using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

// 6.1.7.1 Property Attributes, https://tc39.es/ecma262/#sec-property-attributes
// FIXME: [[Value]], [[Get]] [[Set]]
internal sealed record Attributes(bool Writable, bool Enumerable, bool Configurable);

internal sealed class Property
{
    public Property(Value value, Attributes attributes)
    {
        Value = value;
        Attributes = attributes;
    }

    public Value Value { get; set; }
    public Attributes Attributes { get; }
}

// 6.1.7 The Object Type, https://tc39.es/ecma262/#sec-object-type
internal class Object : Value
{
    public Object(Object? prototype)
    {
        Prototype = prototype ?? this;
        DataProperties = new();
    }

    override public bool IsObject() { return true; }
    override public ValueType Type() {  return ValueType.Object; }

    // 7.3.2 Get ( O, P ), https://tc39.es/ecma262/#sec-get-o-p
    static public Completion Get(Object O, string P)
    {
        // 1. Return ? O.[[Get]](P, O).
        return O.Get(P, O);
    }

    // 7.3.4 Set ( O, P, V, Throw ), https://tc39.es/ecma262/#sec-set-o-p-v-throw
    static public Completion Set(VM vm, Object O, string P, Value V, bool Throw)
    {
        // 1. Let success be ? O.[[Set]](P, V, O).
        var success = O.Set(P, V, O);
        if (success.IsAbruptCompletion()) return success;

        // FIXME: Throw an actual TypeError object
        // 2. If success is false and Throw is true, throw a TypeError exception.
        var asBoolean = (success.Value as Boolean)!;
        if (!asBoolean.Value && Throw)
        {
            return Completion.ThrowCompletion(new String($"Failed to set {P}"));
        }

        // 3. Return UNUSED.
        return Completion.NormalCompletion(vm.Empty);
    }

    // 7.3.12 HasProperty ( O, P ), https://tc39.es/ecma262/#sec-hasproperty
    static public Completion HasProperty(Object O, string P)
    {
        // 1. Return ? O.[[HasProperty]](P).
        return O.HasProperty(P);
    }

    // 10.1.5 [[GetOwnProperty]] ( P )
    public Completion GetOwnProperty(string P)
    {
        // 1. Return OrdinaryGetOwnProperty(O, P).
        return OrdinaryGetOwnProperty(this, P);
    }

    // 10.1.5.1 OrdinaryGetOwnProperty ( O, P ), https://tc39.es/ecma262/#sec-ordinarygetownproperty
    private Completion OrdinaryGetOwnProperty(Object O, string P)
    {
        // 1. If O does not have an own property with key P, return undefined.
        if (!O.DataProperties.ContainsKey(P))
        {
            // FIXME: Remove new Undefined()
            return Completion.NormalCompletion(new Undefined());
        }

        // FIXME: 2. Let D be a newly created Property Descriptor with no fields.
        // FIXME: 3. Let X be O's own property whose key is P.
        // FIXME: 4. If X is a data property, then
        // FIXME: a. Set D.[[Value]] to the value of X's [[Value]] attribute.
        // FIXME: b. Set D.[[Writable]] to the value of X's [[Writable]] attribute.
        // FIXME: 5. Else,
        // FIXME: a. Assert: X is an accessor property.
        // FIXME: b. Set D.[[Get]] to the value of X's [[Get]] attribute.
        // FIXME: c. Set D.[[Set]] to the value of X's [[Set]] attribute.
        // FIXME: 6. Set D.[[Enumerable]] to the value of X's [[Enumerable]] attribute.
        // FIXME: 7. Set D.[[Configurable]] to the value of X's [[Configurable]] attribute.
        // FIXME: 8. Return D.
        return Completion.NormalCompletion(O.DataProperties[P].Value);
    }

    // 10.1.7 [[HasProperty]] ( P ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-hasproperty-p
    private Completion HasProperty(string P)
    {
        // 1. Return ? OrdinaryHasProperty(O, P).
        return OrdinaryHasProperty(this, P);
    }

    // 10.1.7.1 OrdinaryHasProperty ( O, P ), https://tc39.es/ecma262/#sec-ordinaryhasproperty
    static public Completion OrdinaryHasProperty(Object O, string P)
    {
        // 1. Let hasOwn be ? O.[[GetOwnProperty]](P).
        var hasOwn = O.GetOwnProperty(P);
        if (hasOwn.IsAbruptCompletion()) return hasOwn;

        // FIXME: 2. If hasOwn is not undefined, return true.
        return Completion.NormalCompletion(new Boolean(!hasOwn.Value.IsUndefined()));

        // FIXME: 3. Let parent be ? O.[[GetPrototypeOf]]().
        // FIXME: 4. If parent is not null, then
        // FIXME: a. Return ? parent.[[HasProperty]](P).
        // FIXME: 5. Return false.
    }

    // 10.1.8 [[Get]] ( P, Receiver ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-get-p-receiver
    private Completion Get(string P, Object receiver)
    {
        // 1. Return ? OrdinaryGet(O, P, Receiver).
        return OrdinaryGet(this, P, receiver);
    }

    // 10.1.8.1 OrdinaryGet ( O, P, Receiver ), https://tc39.es/ecma262/#sec-ordinaryget
    static public Completion OrdinaryGet(Object O, string P, Object receiver)
    {
        // FIXME: 1. Let desc be ? O.[[GetOwnProperty]](P).
        // FIXME: 2. If desc is undefined, then
        // FIXME: a. Let parent be ? O.[[GetPrototypeOf]]().
        // FIXME: b. If parent is null, return undefined.
        // FIXME: c. Return ? parent.[[Get]](P, Receiver).

        // FIXME: 3. If IsDataDescriptor(desc) is true, return desc.[[Value]].
        return Completion.NormalCompletion(O.DataProperties[P].Value);

        // FIXME: 4. Assert: IsAccessorDescriptor(desc) is true.
        // FIXME: 5. Let getter be desc.[[Get]].
        // FIXME: 6. If getter is undefined, return undefined.
        // FIXME: 7. Return ? Call(getter, Receiver).
    }

    // 10.1.9 [[Set]] ( P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-set-p-v-receiver
    private Completion Set(string P, Value V, Object receiver)
    {
        // 1. Return ? OrdinarySet(O, P, V, Receiver).
        return OrdinarySet(this, P, V, receiver);
    }

    // 10.1.9.1 OrdinarySet ( O, P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinaryset
    static public Completion OrdinarySet(Object O, string P, Value V, Object receiver)
    {
        // FIXME: 1. Let ownDesc be ? O.[[GetOwnProperty]](P).
        // FIXME: 2. Return ? OrdinarySetWithOwnDescriptor(O, P, V, Receiver, ownDesc).
        if (receiver.DataProperties.ContainsKey(P))
        {
            var property = receiver.DataProperties[P];
            property.Value = V;
        }
        else
        {
            receiver.DataProperties[P] = new Property(V, new Attributes(true, false, false));
        }

        return Completion.NormalCompletion(new Boolean(true));
    }

    // FIXME: Accessor Attributes
    public Object Prototype { get; }
    public Dictionary<string, Property> DataProperties { get; }
}
