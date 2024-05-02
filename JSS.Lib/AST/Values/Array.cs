using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST.Values;

// 10.4.2 Array Exotic Objects, https://tc39.es/ecma262/#sec-array-exotic-objects
internal sealed class Array : Object
{
    public Array(Object? prototype) : base(prototype)
    {
    }

    // 10.4.2.1 [[DefineOwnProperty]] ( P, Desc ), https://tc39.es/ecma262/#sec-array-exotic-objects-defineownproperty-p-desc
    internal override Completion DefineOwnProperty(string P, Property desc)
    {
        // 1. If P is "length", then
        if (P == "length")
        {
            // a. Return ? ArraySetLength(A, Desc).
            return ArraySetLength(desc);
        }
        // 2. Else if P is an array index, then
        else if (IsArrayIndex(P))
        {
            // a. Let lengthDesc be OrdinaryGetOwnProperty(A, "length").
            var lengthDesc = OrdinaryGetOwnProperty(this, "length").Value.AsProperty();

            // FIXME: b. Assert: IsDataDescriptor(lengthDesc) is true.
            // c. Assert: lengthDesc.[[Configurable]] is false.
            Assert(!lengthDesc.Attributes.Configurable, "c. Assert: lengthDesc.[[Configurable]] is false.");

            // d. Let length be lengthDesc.[[Value]].
            var length = lengthDesc.Value.AsNumber();

            // e. Assert: length is a non-negative integral Number.
            Assert(length >= 0 && double.IsInteger(length), "e. Assert: length is a non-negative integral Number.");

            // f. Let index be ! ToUint32(P).
            var pString = new String(P);
            var index = MUST(pString.ToUint32());

            // g. If index ≥ length and lengthDesc.[[Writable]] is false, return false.
            if (index >= length && !lengthDesc.Attributes.Writable) return false;

            // h. Let succeeded be ! OrdinaryDefineOwnProperty(A, P, Desc).
            var succeeded = MUST(OrdinaryDefineOwnProperty(this, P, desc)).AsBoolean();

            // i. If succeeded is false, return false.
            if (!succeeded) return false;

            // j. If index ≥ length, then
            if (index >= length)
            {
                // i. Set lengthDesc.[[Value]] to index + 1𝔽.
                lengthDesc.Value = index + 1;

                // ii. Set succeeded to ! OrdinaryDefineOwnProperty(A, "length", lengthDesc).
                succeeded = MUST(OrdinaryDefineOwnProperty(this, "length", lengthDesc)).AsBoolean();

                // iii. Assert: succeeded is true.
                Assert(succeeded, "iii. Assert: succeeded is true.");
            }

            // k. Return true.
            return true;
        }

        // 3. Return ? OrdinaryDefineOwnProperty(A, P, Desc).
        return OrdinaryDefineOwnProperty(this, P, desc);
    }

    // 10.4.2.2 ArrayCreate ( length [ , proto ] )
    static public AbruptOr<Array> ArrayCreate(VM vm, int length, Object? prototype = null)
    {
        // FIXME: 1. If length > 2**32 - 1, throw a RangeError exception.

        // 2. If proto is not present, set proto to %Array.prototype%.
        prototype ??= vm.ArrayPrototype;

        // 3. Let A be MakeBasicObject(« [[Prototype]], [[Extensible]] »).
        // 4. Set A.[[Prototype]] to proto.
        // FIXME: 5. Set A.[[DefineOwnProperty]] as specified in 10.4.2.1.
        var A = new Array(prototype);

        // 6. Perform ! OrdinaryDefineOwnProperty(A, "length", PropertyDescriptor { [[Value]]: 𝔽(length), [[Writable]]: true, [[Enumerable]]: false, [[Configurable]]: false }).
        MUST(OrdinaryDefineOwnProperty(A, "length", new Property(length, new(true, false, false))));

        // 7. Return A.
        return A;
    }

    // 10.4.2.4 ArraySetLength ( A, Desc ), https://tc39.es/ecma262/#sec-arraysetlength
    private Completion ArraySetLength(Property desc)
    {
        // FIXME: 1. If Desc does not have a [[Value]] field, then
        // FIXME: a. Return ! OrdinaryDefineOwnProperty(A, "length", Desc).

        // 2. Let newLenDesc be a copy of Desc.
        var newLenDesc = desc.Copy();

        // 3. Let newLen be ? ToUint32(Desc.[[Value]]).
        var newLen = desc.Value.ToUint32();
        if (newLen.IsAbruptCompletion()) return newLen.Completion;

        // FIXME: 4. Let numberLen be ? ToNumber(Desc.[[Value]]).
        // FIXME: 5. If SameValueZero(newLen, numberLen) is false, throw a RangeError exception.

        // 6. Set newLenDesc.[[Value]] to newLen.
        newLenDesc.Value = newLen.Value;

        // 7. Let oldLenDesc be OrdinaryGetOwnProperty(A, "length").
        // FIXME: OrdinaryGetOwnProperty shouldn't return a completion
        var oldLenDesc = OrdinaryGetOwnProperty(this, "length").Value.AsProperty();

        // FIXME: 8. Assert: IsDataDescriptor(oldLenDesc) is true.
        // 9. Assert: oldLenDesc.[[Configurable]] is false.
        Assert(!oldLenDesc.Attributes.Configurable, "9. Assert: oldLenDesc.[[Configurable]] is false.");

        // 10. Let oldLen be oldLenDesc.[[Value]].
        var oldLen = oldLenDesc.Value.AsNumber();

        // FIXME: 11. If newLen ≥ oldLen, then
        // a. Return ! OrdinaryDefineOwnProperty(A, "length", newLenDesc).
        return MUST(OrdinaryDefineOwnProperty(this, "length", newLenDesc));

        // FIXME: 12. If oldLenDesc.[[Writable]] is false, return false.
        // FIXME: 13. If (newLenDesc does not have a [[Writable]] field) or newLenDesc.[[Writable]] is true, then
        // FIXME: a. Let newWritable be true.
        // FIXME: 14. Else,
        // FIXME: a. NOTE: Setting the [[Writable]] attribute to false is deferred in case any elements cannot be deleted.
        // FIXME: b. Let newWritable be false.
        // FIXME: c. Set newLenDesc.[[Writable]] to true.
        // FIXME: 15. Let succeeded be ! OrdinaryDefineOwnProperty(A, "length", newLenDesc).
        // FIXME: 16. If succeeded is false, return false.
        // FIXME: 17. For each own property key P of A such that P is an array index and ! ToUint32(P) ≥ newLen, in descending numeric index order, do
        // FIXME: a. Let deleteSucceeded be ! A.[[Delete]](P).
        // FIXME: b. If deleteSucceeded is false, then
        // FIXME: i. Set newLenDesc.[[Value]] to ! ToUint32(P) + 1𝔽.
        // FIXME: ii. If newWritable is false, set newLenDesc.[[Writable]] to false.
        // FIXME: iii. Perform ! OrdinaryDefineOwnProperty(A, "length", newLenDesc).
        // FIXME: iv. Return false.
        // FIXME: 18. If newWritable is false, then
        // FIXME: a. Set succeeded to ! OrdinaryDefineOwnProperty(A, "length", PropertyDescriptor { [[Writable]]: false }).
        // FIXME: b. Assert: succeeded is true.
        // FIXME: 19. Return true.
    }

    // array index, https://tc39.es/ecma262/#array-index
    private bool IsArrayIndex(string P)
    {
        // FIXME: An array index is an integer index n such that CanonicalNumericIndexString(n) returns an integral Number in the inclusive interval from +0𝔽 to 𝔽(2**32 - 2).
        return P.All(char.IsAsciiDigit);
    }
}
