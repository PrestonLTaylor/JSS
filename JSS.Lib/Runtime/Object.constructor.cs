using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;
using Object = JSS.Lib.AST.Values.Object;

namespace JSS.Lib.Runtime;

internal class ObjectConstructor : Object, ICallable, IConstructable
{
    private ObjectConstructor(Object? prototype) : base(prototype)
    {
    }

    static public Completion Create()
    {
        // FIXME: The Object constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
        var objectConstructor = new ObjectConstructor(null);

        return Completion.NormalCompletion(objectConstructor);
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList); 
    }

    // 20.1.1.1 Object ( [ value ] ), https://tc39.es/ecma262/#sec-object-value 
    public Completion Construct(VM vm, List argumentList)
    {
        // FIXME: 1. If NewTarget is neither undefined nor the active function object, then
        // FIXME: a. Return ? OrdinaryCreateFromConstructor(NewTarget, "%Object.prototype%").

        // 2. If value is either undefined or null, FIXME: return OrdinaryObjectCreate(%Object.prototype%).
        var value = argumentList[0];
        if (value.IsUndefined() || value.IsNull())
        {
            return Completion.NormalCompletion(new Object(null));
        }

        // 3. Return ! ToObject(value).
        var asObject = value.ToObject();
        Debug.Assert(asObject.IsNormalCompletion());
        return asObject;
    }
}
