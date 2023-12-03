using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

// 6.1.7.1 Property Attributes, https://tc39.es/ecma262/#sec-property-attributes
// FIXME: [[Value]], [[Get]] [[Set]]
internal sealed record Attributes(bool Writable, bool Enumerable, bool Configurable);

internal struct Property
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

    // 10.1.9 [[Set]] ( P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinary-object-internal-methods-and-internal-slots-set-p-v-receiver
    public Completion Set(string P, Value V, Object receiver)
    {
        // 1. Return ? OrdinarySet(O, P, V, Receiver).
        return OrdinarySet(this, P, V, receiver);
    }

    // 10.1.9.1 OrdinarySet ( O, P, V, Receiver ), https://tc39.es/ecma262/#sec-ordinaryset
    static public Completion OrdinarySet(Object O, string P, Value V, Object receiver)
    {
        // FIXME: 1. Let ownDesc be ? O.[[GetOwnProperty]](P).
        // FIXME: 2. Return ? OrdinarySetWithOwnDescriptor(O, P, V, Receiver, ownDesc).
        var property = receiver.DataProperties[P];
        property.Value = V;
        return Completion.NormalCompletion(new Boolean(true));
    }

    // FIXME: Accessor Attributes
    public Object Prototype { get; }
    public Dictionary<string, Property> DataProperties { get; }
}
