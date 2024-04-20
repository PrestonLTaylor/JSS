namespace JSS.Lib.Runtime;

internal class NativeErrorPrototype : Object
{
    // Each NativeError prototype object has a [[Prototype]] internal slot whose value is %Error.prototype%.
    public NativeErrorPrototype(ErrorPrototype prototype) : base(prototype)
    {
    }

    // 20.5.6.3 Properties of the NativeError Prototype Objects, https://tc39.es/ecma262/#sec-properties-of-the-nativeerror-prototype-objects
    public void Initialize(Object constructor, string name)
    {
        // 20.5.6.3.1 NativeError.prototype.constructor, The initial value of the "constructor" property of the prototype for a given NativeError constructor is the constructor itself.
        DataProperties.Add("constructor", new(constructor, new(false, false, false)));

        // 20.5.6.3.2 NativeError.prototype.message, The initial value of the "message" property of the prototype for a given NativeError constructor is the empty String.
        DataProperties.Add("message", new("", new(false, false, false)));

        // 20.5.6.3.3 NativeError.prototype.name, The initial value of the "name" property of the prototype for a given NativeError constructor
        // is the String value consisting of the name of the constructor (the name used instead of NativeError).
        DataProperties.Add("name", new(name, new(false, false, false)));
    }
}
