using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 23.1.1 The Array Constructor, https://tc39.es/ecma262/#sec-array-constructor
internal sealed class ArrayConstructor : Object
{
    // The Array constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public ArrayConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm)
    {
        // 23.1.2.4 Array.prototype, https://tc39.es/ecma262/#sec-array.prototype
        DataProperties.Add("prototype", new(realm.ArrayPrototype, new(true, false, true)));
    }
}
