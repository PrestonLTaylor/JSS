namespace JSS.Lib.Runtime;

internal sealed class ArrayPrototype : Object
{
    // The Array prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public ArrayPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }
}
