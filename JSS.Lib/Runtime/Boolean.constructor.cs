using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.3.1 The Boolean Constructor, https://tc39.es/ecma262/#sec-boolean-constructor
internal sealed class BooleanConstructor : Object, ICallable, IConstructable
{
    // The Boolean constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public BooleanConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm)
    {
        // 20.3.2.1 Boolean.prototype, The initial value of Boolean.prototype is the Boolean prototype object.
        DataProperties.Add("prototype", new(realm.BooleanPrototype, new(false, false, false)));
    }

    // 20.3.1.1 Boolean ( value ), https://tc39.es/ecma262/#sec-boolean-constructor-boolean-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The);
    }

    // 20.3.1.1 Boolean ( value ), https://tc39.es/ecma262/#sec-boolean-constructor-boolean-value
    public Completion Construct(VM vm, List argumentsList, Object newTarget)
    {
        // 1. Let b be ToBoolean(value).
        var b = argumentsList[0].ToBoolean();

        // 2. If NewTarget is undefined, return b.
        if (newTarget.IsUndefined()) return b;

        // FIXME: 3. Let O be ? OrdinaryCreateFromConstructor(NewTarget, "%Boolean.prototype%", « [[BooleanData]] »).
        // 4. Set O.[[BooleanData]] to b.
        var O = new BooleanObject(vm.ObjectPrototype, b);

        // 5. Return O.
        return O;
    }
}
