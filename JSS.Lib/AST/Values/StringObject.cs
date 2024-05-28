using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

// 22.1.4 Properties of String Instances, https://tc39.es/ecma262/#sec-properties-of-string-instances
internal sealed class StringObject : Object
{
    // FIXME: String instances are String exotic objects and have the internal methods specified for such objects.
    // FIXME: String instances have a "length" property, and a set of enumerable properties with integer-indexed names.
    // String instances inherit properties from the String prototype object.
    // 10.4.3.4 StringCreate ( value, prototype ), https://tc39.es/ecma262/#sec-stringcreate
    public StringObject(VM vm, String value, Object prototype) : base(prototype)
    {
        // 1. Let S be MakeBasicObject(« [[Prototype]], [[Extensible]], [[StringData]] »).
        // 2. Set S.[[Prototype]] to prototype.

        // 3. Set S.[[StringData]] to value.
        StringData = value;

        // FIXME: 4. Set S.[[GetOwnProperty]] as specified in 10.4.3.1.
        // FIXME: 5. Set S.[[DefineOwnProperty]] as specified in 10.4.3.2.
        // FIXME: 6. Set S.[[OwnPropertyKeys]] as specified in 10.4.3.3.

        // 7. Let length be the length of value.
        var length = value.Value.Length;

        // 8. Perform ! DefinePropertyOrThrow(S, "length", PropertyDescriptor { [[Value]]: 𝔽(length), [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: false }).
        MUST(DefinePropertyOrThrow(vm, this, "length", new(length, new(false, false, false))));

        // 9. Return S.
    }

    // [[StringData]]
    public String StringData { get; }
}
