using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 22.1.1 The String Constructor, https://tc39.es/ecma262/#sec-string-constructor
internal class StringConstructor : Object, ICallable, IConstructable
{
    // The String constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public StringConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm)
    {
        // 22.1.2.3 String.prototype, https://tc39.es/ecma262/#sec-string.prototype
        // This property has the attributes { [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: false }.
        InternalDefineProperty("prototype", realm.StringPrototype, new(false, false, false));
    }

    // 22.1.1.1 String ( value ), https://tc39.es/ecma262/#sec-string-constructor-string-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The);
    }

    // 22.1.1.1 String ( value ), https://tc39.es/ecma262/#sec-string-constructor-string-value
    public Completion Construct(VM vm, List argumentsList, Object newTarget)
    {
        // 1. If value is not present, then
        string s;
        if (argumentsList.Count == 0)
        {
            s = "";
        }
        // 2. Else,
        else
        {
            // FIXME: a. If NewTarget is undefined and value is a Symbol, return SymbolDescriptiveString(value).

            // b. Let s be ? ToString(value).
            var value = argumentsList.Values[0];
            var abruptOrS = value.ToStringJS(vm);
            if (abruptOrS.IsAbruptCompletion()) return abruptOrS.Completion;
            s = abruptOrS.Value;
        }

        // 3. If NewTarget is undefined, return s.
        if (newTarget.IsUndefined()) return s;

        // 4. Return StringCreate(s, FIXME: ? GetPrototypeFromConstructor(NewTarget, "%String.prototype%")).
        return new StringObject(vm, s, vm.StringPrototype);
    }
}
