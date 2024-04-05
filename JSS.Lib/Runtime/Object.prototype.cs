using JSS.Lib.AST.Values;

namespace JSS.Lib.Runtime;

// 20.1.3 Properties of the Object Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-object-prototype-object
internal class ObjectPrototype : Object
{
    // The Object prototype object has a [[Prototype]] internal slot whose value is null.
    public ObjectPrototype() : base(null)
    {
        DataProperties.Add("constructor", new Property(ObjectConstructor.The, new Attributes(true, false, true)));
    }

    static public ObjectPrototype The
    {
        get
        {
            return _prototype;
        }
    }
    static readonly private ObjectPrototype _prototype = new();
}
