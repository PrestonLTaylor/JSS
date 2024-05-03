using JSS.Lib.Runtime;

namespace JSS.Lib.AST.Values;

// 22.1.4 Properties of String Instances, https://tc39.es/ecma262/#sec-properties-of-string-instances
internal sealed class StringObject : Object
{
    // FIXME: String instances are String exotic objects and have the internal methods specified for such objects.
    // FIXME: String instances inherit properties from the String prototype object.
    // FIXME: String instances have a "length" property, and a set of enumerable properties with integer-indexed names.
    public StringObject(ObjectPrototype prototype, String value) : base(prototype)
    {
        StringData = value;
    }

    // [[StringData]]
    public String StringData { get; }
}
