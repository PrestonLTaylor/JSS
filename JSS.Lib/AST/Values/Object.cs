namespace JSS.Lib.AST.Values;

// 6.1.7.1 Property Attributes, https://tc39.es/ecma262/#sec-property-attributes
// FIXME: [[Value]], [[Get]] [[Set]]
internal sealed record Attributes(bool Writable, bool Enumerable, bool Configurable);

internal sealed record Property(Value Value, Attributes Attributes);

// 6.1.7 The Object Type, https://tc39.es/ecma262/#sec-object-type
internal sealed class Object : Value
{
    public Object(Object? prototype)
    {
        Prototype = prototype ?? this;
        DataProperties = new();
    }

    override public bool IsObject() { return true; }
    override public ValueType Type() {  return ValueType.Object; }

    // FIXME: Accessor Attributes
    public Object Prototype { get; }
    public Dictionary<string, Property> DataProperties { get; }
}
