using JSS.Lib.Runtime;

namespace JSS.Lib.AST.Values;

// 20.3.4 Properties of Boolean Instances, https://tc39.es/ecma262/#sec-properties-of-boolean-instances
internal class BooleanObject : Object
{
    // FIXME: Boolean instances are ordinary objects that inherit properties from the Boolean prototype object.
    public BooleanObject(ObjectPrototype prototype, Boolean value) : base(prototype)
    {
        BooleanData = value;
    }

    // [[BooleanData]]
    public Boolean BooleanData { get; }
}
