using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

internal sealed class ErrorPrototype : Object
{
    // The Error prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public ErrorPrototype(ObjectPrototype prototype) : base(prototype)
    {
    }

    // 20.5.3 Properties of the Error Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-error-prototype-object
    public void Initialize(Realm realm)
    {
        // 20.5.3.1 Error.prototype.constructor, The initial value of Error.prototype.constructor is %Error%.
        DataProperties.Add("constructor", new Property(realm.ErrorConstructor, new(true, false, false)));

        // 20.5.3.2 Error.prototype.message, The initial value of Error.prototype.message is the empty String.
        DataProperties.Add("message", new Property("", new(true, false, false)));

        // 20.5.3.3 Error.prototype.name, The initial value of Error.prototype.name is "Error".
        DataProperties.Add("name", new Property("Error", new(true, false, false)));
    }
}
