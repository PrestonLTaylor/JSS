using JSS.Lib.Execution;
using System.Diagnostics;
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

    // 6.2.5.2 IsUnresolvableReference ( V ), https://tc39.es/ecma262/#sec-isunresolvablereference
    public bool IsUnresolvableReference()
    {
        // 1. If V.[[Base]] is UNRESOLVABLE, return true; otherwise return false.
        return Base is null;
    }

    // 6.2.5.8 InitializeReferencedBinding ( V, W ), https://tc39.es/ecma262/#sec-initializereferencedbinding
    public Completion InitializeReferencedBinding(Value W)
    {
        // 1. Assert: IsUnresolvableReference(V) is false.
        Debug.Assert(!IsUnresolvableReference());

        // 2. Let base be V.[[Base]].

        // 3. Assert: base is an Environment Record.
        Debug.Assert(Base is not null);

        // 4. Return ? base.InitializeBinding(V.[[ReferencedName]], W).
        return Base.InitializeBinding(ReferencedName, W);
    }

    // FIXME: Base can have a ECMAScript language value
    public Environment? Base { get; }

    // FIXME: ReferencedName can be a Symbol or Private Name
    public string ReferencedName { get; }

    // FIXME: [[Strict]]

    public Value ThisValue { get; }
}
