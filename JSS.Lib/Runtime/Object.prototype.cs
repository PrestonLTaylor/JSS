using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.1.3 Properties of the Object Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-object-prototype-object
internal class ObjectPrototype : Object
{
    // The Object prototype object has a [[Prototype]] internal slot whose value is null.
    public ObjectPrototype() : base(null)
    {
    }

    public void Initialize(Realm realm)
    {
        DataProperties.Add("constructor", new Property(realm.ObjectConstructor, new Attributes(true, false, true)));
    }
}
