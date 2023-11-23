using JSS.Lib.Execution;

namespace JSS.Lib.AST.Value;

// FIXME: This is a very inefficient way of storing JS values.
// 6.1 ECMAScript Language Types, https://tc39.es/ecma262/#sec-ecmascript-language-types
internal abstract class Value
{
    virtual public bool IsUndefined() { return false; }
    virtual public bool IsNull() { return false; }
    virtual public bool IsBoolean() { return false; }
    virtual public bool IsString() { return false; }
    virtual public bool IsSymbol() { return false; }
    virtual public bool IsNumber() { return false; }
    virtual public bool IsBigInt() { return false; }
    virtual public bool IsObject() { return false; }

    // 7.1.1 ToPrimitive ( input FIXME: [ , preferredType ] ), https://tc39.es/ecma262/#sec-toprimitive
    public Completion ToPrimitive()
    {
        // FIXME: 1 .If input is an Object, then
        // FIXME: a. Let exoticToPrim be ? GetMethod(input, @@toPrimitive).
        // FIXME: b. If exoticToPrim is not undefined, then
        // FIXME: i. If preferredType is not present, then
        // FIXME: 1. Let hint be "default".
        // FIXME: ii. Else if preferredType is STRING, then
        // FIXME: 1. Let hint be "string".
        // FIXME: iii. Else,
        // FIXME: 1. Assert: preferredType is NUMBER.
        // FIXME: 2. Let hint be "number".
        // FIXME: iv. Let result be ? Call(exoticToPrim, input, « hint »).
        // FIXME: v. If result is not an Object, return result.
        // FIXME:  vi. Throw a TypeError exception.
        // FIXME: c. If preferredType is not present, let preferredType be NUMBER.
        // FIXME: d. Return? OrdinaryToPrimitive(input, preferredType).

        // 2. Return input.
        return Completion.NormalCompletion(this);
    }
}
