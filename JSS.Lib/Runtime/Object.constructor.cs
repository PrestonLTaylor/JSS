using JSS.Lib.AST.Values;
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
        DataProperties.Add("length", new Property(1, new Attributes(true, false, true)));

        // 20.1.2.21 Object.prototype, The initial value of Object.prototype is the Object prototype object.
        // This property has the attributes { [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: false }.
        DataProperties.Add("prototype", new Property(realm.ObjectPrototype, new(false, false, false)));

        // 20.1.2.8 Object.getOwnPropertyDescriptor ( O, P ), https://tc39.es/ecma262/#sec-object.getownpropertydescriptor
        var getOwnPropertyDescriptorBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, getOwnPropertyDescriptor);
        DataProperties.Add("getOwnPropertyDescriptor", new Property(getOwnPropertyDescriptorBuiltin, new(true, false, true)));
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList); 
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value 
    public Completion Construct(VM vm, List argumentList)
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

    // 20.1.2.8 Object.getOwnPropertyDescriptor ( O, P ), https://tc39.es/ecma262/#sec-object.getownpropertydescriptor
    private Completion getOwnPropertyDescriptor(VM vm, Value? thisArgument, List argumentList)
    {
        // 1. Let obj be ? ToObject(O).
        var obj = argumentList[0].ToObject(vm);
        if (obj.IsAbruptCompletion()) return obj.Completion;

        // 2. Let key be ? ToPropertyKey(P).
        var key = argumentList[1].ToPropertyKey();
        if (key.IsAbruptCompletion()) return key;

        // 3. Let desc be ? obj.[[GetOwnProperty]](key).
        // FIXME: When we implement symbols, we can't rely on AsString here
        var desc = obj.Value.GetOwnProperty(key.Value.AsString());
        if (desc.IsAbruptCompletion()) return desc;

        // 4. Return FromPropertyDescriptor(desc).
        return desc.Value.FromPropertyDescriptor(vm);
    }
}
