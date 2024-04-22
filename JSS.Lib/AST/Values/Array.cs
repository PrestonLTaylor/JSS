using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

// 10.4.2 Array Exotic Objects, https://tc39.es/ecma262/#sec-array-exotic-objects
internal sealed class Array : Object
{
    public Array(Object? prototype) : base(prototype)
    {
    }

    // 10.4.2.2 ArrayCreate ( length [ , proto ] )
    static public AbruptOr<Array> ArrayCreate(int length, Object? prototype = null)
    {
        // FIXME: 1. If length > 2**32 - 1, throw a RangeError exception.
        // FIXME: 2. If proto is not present, set proto to %Array.prototype%.

        // 3. Let A be MakeBasicObject(« [[Prototype]], [[Extensible]] »).
        // 4. Set A.[[Prototype]] to proto.
        // FIXME: 5. Set A.[[DefineOwnProperty]] as specified in 10.4.2.1.
        var A = new Array(prototype);

        // 6. Perform ! OrdinaryDefineOwnProperty(A, "length", PropertyDescriptor { [[Value]]: 𝔽(length), [[Writable]]: true, [[Enumerable]]: false, [[Configurable]]: false }).
        MUST(OrdinaryDefineOwnProperty(A, "length", new Property(length, new(true, false, false))));

        // 7. Return A.
        return A;
    }
}
