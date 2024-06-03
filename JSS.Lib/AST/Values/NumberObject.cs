using JSS.Lib.Runtime;

namespace JSS.Lib.AST.Values;

// 21.1.4 Properties of Number Instances, https://tc39.es/ecma262/#sec-properties-of-number-instances
internal sealed class NumberObject : Object
{
    // Number instances are ordinary objects that inherit properties from the Number prototype object.
    public NumberObject(NumberPrototype prototype, Number value) : base(prototype)
    {
        NumberData = value;
    }

    // [[NumberData]]
    public Number NumberData { get; }
}
