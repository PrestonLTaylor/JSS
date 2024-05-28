using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 22.1.3 Properties of the String Prototype Object, https://tc39.es/ecma262/multipage/text-processing.html#sec-properties-of-the-string-prototype-object
// FIXME: The String prototype object is a String exotic object and has the internal methods specified for such objects.
internal sealed class StringPrototype : Object
{
    // The String prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public StringPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm)
    {
        // 22.1.3.6 String.prototype.constructor, The initial value of String.prototype.constructor is %String%.
        DataProperties.Add("constructor", new(realm.StringConstructor, new(true, false, true)));
    }
}
