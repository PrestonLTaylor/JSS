using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.3.3 Properties of the Boolean Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-boolean-prototype-object
// The Boolean prototype object is itself a Boolean object; it has a [[BooleanData]] internal slot with the value false.
internal sealed class BooleanPrototype : BooleanObject 
{
    // The Boolean prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public BooleanPrototype(ObjectPrototype prototype) : base(prototype, false)
    {
    }

    public void Initialize(Realm realm)
    {
        // 20.3.3.1 Boolean.prototype.constructor, The initial value of Boolean.prototype.constructor is %Boolean%.
        DataProperties.Add("constructor", new(realm.BooleanConstructor, new(true, false, true)));
    }
}
