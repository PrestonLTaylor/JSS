using Environment = JSS.Lib.Execution.Environment;

namespace JSS.Lib.AST.Values;

// 6.2.5 The Reference Record Specification Type, https://tc39.es/ecma262/#sec-reference-record-specification-type
internal class Reference : Value
{
    public Reference(Environment? @base, string referencedName, Value thisValue)
    {
        Base = @base;
        ReferencedName = referencedName;
        ThisValue = thisValue;
    }

    override public bool IsReference() { return true; }
    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of Reference"); }

    static public Reference Unresolvable(string referencedName, Value thisValue)
    {
        return new Reference(null, referencedName, thisValue);
    }

    static public Reference Resolvable(Environment @base, string referencedName, Value thisValue)
    {
        return new Reference(@base, referencedName, thisValue);
    }

    // FIXME: Base can have a ECMAScript language value
    public Environment? Base { get; }

    // FIXME: ReferencedName can be a Symbol or Private Name
    public string ReferencedName { get; }

    // FIXME: [[Strict]]

    public Value ThisValue { get; }
}
