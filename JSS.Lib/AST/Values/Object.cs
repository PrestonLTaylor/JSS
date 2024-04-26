using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST.Values;

// 6.1.7 The Object Type, https://tc39.es/ecma262/#sec-object-type
public class Object : Value
{
    internal Object(Object? prototype)
    {
        Prototype = prototype;
        DataProperties = new();
    }

    override public bool IsObject() { return true; }
    override public ValueType Type() { return ValueType.Object; }

    public string ToString(VM vm)
    {
        var toString = Get(this, "toString");
        if (toString.IsAbruptCompletion()) return "Object";

        if (toString.Value.HasInternalCall())
        {
            var toStringFunc = toString.Value.AsCallable();
            var completion = toStringFunc.Call(vm, this, new List());
            if (completion.IsNormalCompletion()) return completion.Value.AsString();
        }

        return "Object";
    }

    // 7.1.1.1 OrdinaryToPrimitive ( O, hint ), https://tc39.es/ecma262/#sec-ordinarytoprimitive
    internal Completion OrdinaryToPrimitive(VM vm, PreferredType hint)
    {
        // 1. If hint is STRING, then
        List<string> methodNames;
        if (hint == PreferredType.STRING)
        {
            // a. Let methodNames be « "toString", "valueOf" ».
            methodNames = new() { "toString", "valueOf" };
        }
        // 2. Else,
        else
        {
            // a. Let methodNames be « "valueOf", "toString" ».
            methodNames = new() { "valueOf", "toString" };
        }

        // 3. For each element name of methodNames, do
        foreach (var name in methodNames)
        {
            // a. Let method be ? Get(O, name).
            var method = Get(this, name);
            if (method.IsAbruptCompletion()) return method;

            // b. If IsCallable(method) is true, then
            if (method.Value.IsCallable())
            {
                // i. Let result be ? Call(method, O).
                var callable = method.Value.AsCallable();
                var result = callable.Call(vm, this, new());
                if (result.IsAbruptCompletion()) return result;

                // ii. If result is not an Object, return result.
                if (!result.Value.IsObject()) return result;
            }
        }

        // 4. Throw a TypeError exception.
        return ThrowTypeError(vm, RuntimeErrorType.UnableToConvertObjectToPrimitive);
    }

    // 7.3.2 Get ( O, P ), https://tc39.es/ecma262/#sec-get-o-p
    static internal Completion Get(Object O, string P)
    {
        // 1. Return ? O.[[Get]](P, O).
        return O.Get(P, O);
    }

    // 7.3.4 Set ( O, P, V, Throw ), https://tc39.es/ecma262/#sec-set-o-p-v-throw
    static internal Completion Set(VM vm, Object O, string P, Value V, bool Throw)
    {
        // 1. Let success be ? O.[[Set]](P, V, O).
        var success = O.Set(P, V, O);
        if (success.IsAbruptCompletion()) return success;

        // 2. If success is false and Throw is true, throw a TypeError exception.
        var asBoolean = success.Value.AsBoolean();
        if (!asBoolean.Value && Throw)
        {
            return ThrowTypeError(vm, RuntimeErrorType.FailedToSet, P);
        }

        // 3. Return UNUSED.
        return Empty.The;
    }

    // 7.3.5 CreateDataProperty ( O, P, V ), https://tc39.es/ecma262/#sec-createdataproperty
    static internal AbruptOr<bool> CreateDataProperty(Object O, string P, Value V)
    {
        // 1. Let newDesc be the PropertyDescriptor { [[Value]]: V, [[Writable]]: true, [[Enumerable]]: true, [[Configurable]]: true }.
        var newDesc = new Property(V, new(true, true, true));

        // 2. Return ? O.[[DefineOwnProperty]](P, newDesc).
        return O.DefineOwnProperty(P, newDesc);
    }

    // 7.3.6 CreateDataPropertyOrThrow ( O, P, V ), https://tc39.es/ecma262/#sec-createdatapropertyorthrow
    static internal Completion CreateDataPropertyOrThrow(VM vm, Object O, string P, Value V)
    {
        // 1. Let success be ? CreateDataProperty(O, P, V).
        var success = CreateDataProperty(O, P, V);
        if (success.IsAbruptCompletion()) return success.Completion;

        // 2. If success is false, throw a TypeError exception.
        if (!success.Value) return ThrowTypeError(vm, RuntimeErrorType.CouldNotCreateDataProperty, P);

        // 3. Return UNUSED.
        return Empty.The;
    }

    // 7.3.7 CreateNonEnumerableDataPropertyOrThrow ( O, P, V ), https://tc39.es/ecma262/#sec-createnonenumerabledatapropertyorthrow
    static internal void CreateNonEnumerableDataPropertyOrThrow(VM vm, Object O, string P, Value V)
    {
        // 1. Assert: O is an ordinary, FIXME: extensible object with no non-configurable properties.
        Debug.Assert(O.IsObject());

        // 2. Let newDesc be the PropertyDescriptor { [[Value]]: V, [[Writable]]: true, [[Enumerable]]: false, [[Configurable]]: true }.
        var newDesc = new Property(V, new(true, false, true));

        // 3. Perform ! DefinePropertyOrThrow(O, P, newDesc).
        MUST(DefinePropertyOrThrow(vm, O, P, newDesc));

        // 4. Return UNUSED.
    }

    // 7.3.8 DefinePropertyOrThrow ( O, P, desc ), https://tc39.es/ecma262/#sec-definepropertyorthrow
    static internal Completion DefinePropertyOrThrow(VM vm, Object O, string P, Property desc)
    {
        // 1. Let success be ? O.[[DefineOwnProperty]](P, desc).
        var success = O.DefineOwnProperty(P, desc);
        if (success.IsAbruptCompletion()) return success;

        // 2. If success is false, throw a TypeError exception.
        var asBoolean = success.Value.AsBoolean();
        if (!asBoolean.Value) return ThrowTypeError(vm, RuntimeErrorType.CouldNotDefineProperty, P, desc.Value);

        // 3. Return UNUSED.
        return Empty.The;
    }

    // 7.3.12 HasProperty ( O, P ), https://tc39.es/ecma262/#sec-hasproperty
    static internal Completion HasProperty(Object O, string P)
    {
        // 1. Return ? O.[[HasProperty]](P).
        return O.HasProperty(P);
    }

    // 7.3.13 HasOwnProperty ( O, P ), https://tc39.es/ecma262/#sec-hasownproperty
    static internal Completion HasOwnProperty(Object O, string P)
    {
        // 1. Let desc be ? O.[[GetOwnProperty]](P).
        var desc = O.GetOwnProperty(P);
        if (desc.IsAbruptCompletion()) return desc;

        // 2. If desc is undefined, return false.
        // 3. Return true.
        return !desc.Value.IsUndefined();
    }

    // 7.3.14 Call ( F, V [ , argumentsList ] )
    static internal Completion Call(VM vm, Value F, Value V, List? argumentsList = null)
    {
        // 1. If argumentsList is not present, set argumentsList to a new empty List.
        argumentsList ??= new List();

        // 2. If IsCallable(F) is false, throw a TypeError exception.
        if (!F.IsCallable())
        {
            return ThrowTypeError(vm, RuntimeErrorType.CallingANonFunction, F.Type());
        }

        // 3. Return ? F.[[Call]](V, argumentsList).
        var asCallable = F.AsCallable();
        return asCallable.Call(vm, V, argumentsList);
    }


    // 7.3.18 LengthOfArrayLike ( obj ), https://tc39.es/ecma262/#sec-lengthofarraylike
    internal AbruptOr<double> LengthOfArrayLike()
    {
        // 1. Return ℝ(? ToLength(? Get(obj, "length"))).
        var getResult = Get(this, "length");
        if (getResult.IsAbruptCompletion()) return getResult;

        return getResult.Value.ToLength();
    }

    // 10.1.1 [[GetPrototypeOf]] ( ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-getprototypeof
    internal Object? GetPrototypeOf()
    {
        // 1. Return OrdinaryGetPrototypeOf(O).
        return OrdinaryGetPrototypeOf(this);
    }

    // 10.1.1.1 OrdinaryGetPrototypeOf ( O ), https://tc39.es/ecma262/#sec-ordinarygetprototypeof
    static internal Object? OrdinaryGetPrototypeOf(Object O)
    {
        // 1. Return O.[[Prototype]].
        return O.Prototype;
    }

    // 10.1.5 [[GetOwnProperty]] ( P ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-getownproperty-p
    internal Completion GetOwnProperty(string P)
    {
        // 1. Return OrdinaryGetOwnProperty(O, P).
        return OrdinaryGetOwnProperty(this, P);
    }

    // 10.1.5.1 OrdinaryGetOwnProperty ( O, P ), https://tc39.es/ecma262/#sec-ordinarygetownproperty
    protected Completion OrdinaryGetOwnProperty(Object O, string P)
    {
        // 1. If O does not have an own property with key P, return undefined.
        if (!O.DataProperties.ContainsKey(P))
        {
            return Undefined.The;
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
        return O.DataProperties[P];
    }

    // 10.1.6 [[DefineOwnProperty]] ( P, Desc ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-defineownproperty-p-desc
    virtual internal Completion DefineOwnProperty(string P, Property desc)
    {
        // 1. Return ? OrdinaryDefineOwnProperty(O, P, Desc).
        return OrdinaryDefineOwnProperty(this, P, desc);
    }

    // 10.1.6.1 OrdinaryDefineOwnProperty ( O, P, Desc ), https://tc39.es/ecma262/#sec-ordinarydefineownproperty
    static internal Completion OrdinaryDefineOwnProperty(Object O, string P, Property desc)
    {
        // FIXME: 1. Let current be ? O.[[GetOwnProperty]](P).
        // FIXME: 2. Let extensible be ? IsExtensible(O).
        // FIXME: 3. Return ValidateAndApplyPropertyDescriptor(O, P, extensible, Desc, current).
        if (O.DataProperties.ContainsKey(P))
        {
            O.DataProperties[P] = desc;
        }
        else
        {
            O.DataProperties.Add(P, desc);
        }

        return true;
    }

    // 10.1.7 [[HasProperty]] ( P ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-hasproperty-p
    private Completion HasProperty(string P)
    {
        // 1. Return ? OrdinaryHasProperty(O, P).
        return OrdinaryHasProperty(this, P);
    }

    // 10.1.7.1 OrdinaryHasProperty ( O, P ), https://tc39.es/ecma262/#sec-ordinaryhasproperty
    static internal Completion OrdinaryHasProperty(Object O, string P)
    {
        // 1. Let hasOwn be ? O.[[GetOwnProperty]](P).
        var hasOwn = O.GetOwnProperty(P);
        if (hasOwn.IsAbruptCompletion()) return hasOwn;

        //  2. If hasOwn is not undefined, return true.
        if (!hasOwn.Value.IsUndefined()) return true;

        // 3. Let parent be ? O.[[GetPrototypeOf]]().
        var parent = O.GetPrototypeOf();

        // 4. If parent is not null, then
        if (parent is not null)
        {
            // a. Return ? parent.[[HasProperty]](P).
            return parent.HasProperty(P);
        }

        // 5. Return false.
        return false;
    }

    // 10.1.8 [[Get]] ( P, Receiver ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-get-p-receiver
    internal Completion Get(string P, Object receiver)
    {
        // 1. Return ? OrdinaryGet(O, P, Receiver).
        return OrdinaryGet(this, P, receiver);
    }

    // 10.1.8.1 OrdinaryGet ( O, P, Receiver ), https://tc39.es/ecma262/#sec-ordinaryget
    static internal Completion OrdinaryGet(Object O, string P, Object receiver)
    {
        // 1. Let desc be ? O.[[GetOwnProperty]](P).
        var desc = O.GetOwnProperty(P);
        if (desc.IsAbruptCompletion()) return desc;

        // 2. If desc is undefined, then
        if (desc.Value.IsUndefined())
        {
            // a. Let parent be ? O.[[GetPrototypeOf]]().
            var parent = O.GetPrototypeOf();

            // b. If parent is null, return undefined.
            if (parent is null) return Undefined.The;

            // c. Return ? parent.[[Get]](P, Receiver).
            return parent.Get(P, receiver);
        }

        // FIXME: 3. If IsDataDescriptor(desc) is true, return desc.[[Value]].
        return O.DataProperties[P].Value;

        // FIXME: 4. Assert: IsAccessorDescriptor(desc) is true.
        // FIXME: 5. Let getter be desc.[[Get]].
        // FIXME: 6. If getter is undefined, return undefined.
        // FIXME: 7. Return ? Call(getter, Receiver).
    }

    // 10.1.9 [[Set]] ( P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-set-p-v-receiver
    internal Completion Set(string P, Value V, Object receiver)
    {
        // 1. Return ? OrdinarySet(O, P, V, Receiver).
        return OrdinarySet(this, P, V, receiver);
    }

    // 10.1.9.1 OrdinarySet ( O, P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinaryset
    static internal Completion OrdinarySet(Object O, string P, Value V, Object receiver)
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

        return true;
    }

    // 10.1.9.2 OrdinarySetWithOwnDescriptor ( O, P, V, Receiver, ownDesc ), https://tc39.es/ecma262/#sec-ordinarysetwithowndescriptor
    internal Completion OrdinarySetWithOwnDescriptor(string P, Value V, Object receiver, Value ownDesc)
    {
        // 1. If ownDesc is undefined, then
        if (ownDesc.IsUndefined())
        {
            // a. Let parent be ? O.[[GetPrototypeOf]]().
            var parent = GetPrototypeOf();

            // b. If parent is not null, then
            if (parent is not null)
            {
                // i. Return ? parent.[[Set]](P, V, Receiver).
                return parent.Set(P, V, receiver);
            }
            // c. Else,
            else
            {
                // i. Set ownDesc to the PropertyDescriptor { [[Value]]: undefined, [[Writable]]: true, [[Enumerable]]: true, [[Configurable]]: true }.
                ownDesc = new Property(Undefined.The, new(true, true, true));
            }
        }

        // FIXME: 2. If IsDataDescriptor(ownDesc) is true, then
        var desc = ownDesc.AsProperty();

        // a. If ownDesc.[[Writable]] is false, return false.
        if (!desc.Attributes.Writable) return false;

        // b. If Receiver is not an Object, return false.
        if (!receiver.IsObject()) return false;

        // c. Let existingDescriptor be ? Receiver.[[GetOwnProperty]](P).
        var existingDescriptor = receiver.GetOwnProperty(P);
        if (existingDescriptor.IsAbruptCompletion()) return existingDescriptor;

        // d. If existingDescriptor is not undefined, then
        if (!existingDescriptor.Value.IsUndefined())
        {
            // FIXME: i. If IsAccessorDescriptor(existingDescriptor) is true, return false.

            // ii. If existingDescriptor.[[Writable]] is false, return false.
            var existingDesc = existingDescriptor.Value.AsProperty();
            if (!existingDesc.Attributes.Writable) return false;

            // FIXME: iii. Let valueDesc be the PropertyDescriptor { [[Value]]: V }.
            existingDesc.Value = V;

            // iv. Return ? Receiver.[[DefineOwnProperty]](P, valueDesc).
            return receiver.DefineOwnProperty(P, existingDesc);
        }
        // e. Else,
        else
        {
            // i. Assert: Receiver does not currently have a property P.
            Debug.Assert(!receiver.DataProperties.ContainsKey(P));

            // ii. Return ? CreateDataProperty(Receiver, P, V).
            var result = CreateDataProperty(receiver, P, V);
            if (result.IsAbruptCompletion()) return result.Completion;
            return result.Value;
        }

        // FIXME: 3. Assert: IsAccessorDescriptor(ownDesc) is true.
        // FIXME: 4.Let setter be ownDesc.[[Set]].
        // FIXME: 5. If setter is undefined, return false.
        // FIXME: 6. Perform ? Call(setter, Receiver, « V »).
        // FIXME: 7.Return true.
    }

    // 10.1.10 [[Delete]] ( P ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-delete-p
    internal AbruptOr<bool> Delete(string P)
    {
        // 1. Return ? OrdinaryDelete(O, P).
        return OrdinaryDelete(P);
    }

    // 10.1.10.1 OrdinaryDelete ( O, P ), https://tc39.es/ecma262/#sec-ordinarydelete
    internal AbruptOr<bool> OrdinaryDelete(string P)
    {
        // 1. Let desc be ? O.[[GetOwnProperty]](P).
        var desc = GetOwnProperty(P);
        if (desc.IsAbruptCompletion()) return desc;

        // 2. If desc is undefined, return true.
        if (desc.Value.IsUndefined()) return true;

        // 3. If desc.[[Configurable]] is true, then
        var asProperty = desc.Value.AsProperty();
        if (asProperty.Attributes.Configurable)
        {
            // a. Remove the own property with name P from O.
            DataProperties.Remove(P);

            // b. Return true.
            return true;
        }

        // 4. Return false.
        return false;
    }

    // 10.1.11 [[OwnPropertyKeys]] ( ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-ownpropertykeys
    internal AbruptOr<List> OwnPropertyKeys()
    {
        // 1. Return OrdinaryOwnPropertyKeys(O).
        return OrdinaryOwnPropertyKeys(this);
    }

    // 10.1.11.1 OrdinaryOwnPropertyKeys ( O ), https://tc39.es/ecma262/#sec-ordinaryownpropertykeys
    static internal List OrdinaryOwnPropertyKeys(Object O)
    {
        // 1. Let keys be a new empty List.
        var keys = new List();

        // FIXME: 2. For each own property key P of O such that P is an array index, in ascending numeric index order, do
        // FIXME: a. Append P to keys.
        // FIXME: 3. For each own property key P of O such that P is a String and P is not an array index, in ascending chronological order of property creation, do
        // FIXME: a. Append P to keys.
        // FIXME: 4. For each own property key P of O such that P is a Symbol, in ascending chronological order of property creation, do
        // FIXME: a. Append P to keys.

        foreach (var (P, _)  in O.DataProperties)
        {
            keys.Add(P);
        }

        // 5. Return keys.
        return keys;
    }

    // FIXME: Accessor Attributes
    internal Object? Prototype { get; }
    internal Dictionary<string, Property> DataProperties { get; }
}
