namespace JSS.Lib.Runtime;

// 21.3 The Math Object, https://tc39.es/ecma262/#sec-math-object
internal sealed class MathObject : Object
{
    // The Math object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public MathObject(ObjectPrototype prototype) : base(prototype)
    {
    }
}
