﻿namespace JSS.Lib.AST.Values;

// 6.1.7.1 Property Attributes, https://tc39.es/ecma262/#sec-property-attributes
// FIXME: [[Get]] [[Set]]
internal sealed class Attributes
{
    public Attributes(bool writable, bool enumerable, bool configurable)
    {
        Writable = writable;
        Enumerable = enumerable;
        Configurable = configurable;
    }

    public bool Writable { get; set;  }
    public bool Enumerable { get; set; }
    public bool Configurable { get; set; }
}

// 6.2.6 The Property Descriptor Specification Type, https://tc39.es/ecma262/#sec-property-descriptor-specification-type
internal sealed class Property : Value
{
    public Property(Value value, Attributes attributes)
    {
        Value = value;
        Attributes = attributes;
    }

    override public bool IsProperty() { return true; }
    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of Property"); }

    public Property Copy()
    {
        return (MemberwiseClone() as Property)!;
    }

    public Value Value { get; set; }
    public Attributes Attributes { get; set; }
}