using JSS.Lib.Runtime;

namespace JSS.Lib.AST.Values;

// 20.3.4 Properties of Boolean Instances, https://tc39.es/ecma262/#sec-properties-of-boolean-instances
internal class BooleanObject : Object
{
    // Boolean instances are ordinary objects that inherit properties from the Boolean prototype object.
    public BooleanObject(BooleanPrototype prototype, Boolean value) : base(prototype)
    {
        BooleanData = value;
    }

    // NOTE: This constructor is used by the BooleanPrototype itself, as the boolean prototype's prototype is the Object prototype
    public BooleanObject(ObjectPrototype prototype, Boolean value) : base(prototype)
    {
        BooleanData = value;
    }

    // [[BooleanData]]
    public Boolean BooleanData { get; }
}
