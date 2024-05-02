using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST.Values;

// 6.2.5 The Reference Record Specification Type, https://tc39.es/ecma262/#sec-reference-record-specification-type
public class Reference : Value
{
    internal Reference(Value? @base, string referencedName, Value thisValue)
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

    static public Reference Resolvable(Value @base, string referencedName, Value thisValue)
    {
        return new Reference(@base, referencedName, thisValue);
    }

    // 6.2.5.1 IsPropertyReference ( V ), https://tc39.es/ecma262/#sec-ispropertyreference
    public bool IsPropertyReference()
    {
        // 1. If V.[[Base]] is UNRESOLVABLE, return false.
        if (IsUnresolvableReference()) return false;

        // 2. If V.[[Base]] is an Environment Record, return false; otherwise return true.
        return !Base!.IsEnvironment();
    }

    // 6.2.5.2 IsUnresolvableReference ( V ), https://tc39.es/ecma262/#sec-isunresolvablereference
    public bool IsUnresolvableReference()
    {
        // 1. If V.[[Base]] is UNRESOLVABLE, return true; otherwise return false.
        return Base is null;
    }

    // 6.2.5.7 GetThisValue ( V ), https://tc39.es/ecma262/#sec-getthisvalue
    internal Value GetThisValue()
    {
        // 1. Assert: IsPropertyReference(V) is true.
        Assert(IsPropertyReference(), "1. Assert: IsPropertyReference(V) is true.");

        // FIXME: (2. If IsSuperReference(V) is true, return V.[[ThisValue]];) otherwise return V.[[Base]].
        return Base ?? Undefined.The;
    }

    // 6.2.5.8 InitializeReferencedBinding ( V, W ), https://tc39.es/ecma262/#sec-initializereferencedbinding
    public Completion InitializeReferencedBinding(VM vm, Value W)
    {
        // 1. Assert: IsUnresolvableReference(V) is false.
        Assert(!IsUnresolvableReference(), "1. Assert: IsUnresolvableReference(V) is false.");

        // 2. Let base be V.[[Base]].

        // 3. Assert: base is an Environment Record.
        Assert(Base!.IsEnvironment(), "3. Assert: base is an Environment Record.");

        // 4. Return ? base.InitializeBinding(V.[[ReferencedName]], W).
        var environment = Base.AsEnvironment();
        return environment.InitializeBinding(vm, ReferencedName, W);
    }

    // FIXME: Base can have a ECMAScript language value
    public Value? Base { get; }

    // FIXME: ReferencedName can be a Symbol or Private Name
    public string ReferencedName { get; }

    // FIXME: [[Strict]]

    public Value ThisValue { get; }
}
