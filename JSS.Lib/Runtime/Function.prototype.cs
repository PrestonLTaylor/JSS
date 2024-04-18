namespace JSS.Lib.Runtime;

// 20.2.3 Properties of the Function Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-function-prototype-object
internal sealed class FunctionPrototype : Object
{
    // The Function prototype has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public FunctionPrototype() : base(ObjectPrototype.The)
    {
    }

    static public FunctionPrototype The
    {
        get
        {
            return _prototype;
        }
    }
    static readonly private FunctionPrototype _prototype = new();
}
