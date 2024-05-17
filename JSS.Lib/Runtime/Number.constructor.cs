using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 21.1.1 The Number Constructor, https://tc39.es/ecma262/#sec-number-constructor
internal sealed class NumberConstructor : Object, ICallable, IConstructable
{
    // The Number constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public NumberConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        return Construct(vm, argumentList, Undefined.The);
    }

    public Completion Construct(VM vm, List argumentsList, Object newTarget)
    {
        // 1. If value is present, then
        double n;
        if (argumentsList.Count != 0)
        {
            // a. Let prim be ? ToNumeric(value).
            var prim = argumentsList[0].ToNumeric(vm);
            if (prim.IsAbruptCompletion()) return prim;

            // FIXME: b. If prim is a BigInt, let n be 𝔽(ℝ(prim)).
            // c. Otherwise, let n be prim.
            n = prim.Value.AsNumber();
        }
        // 2. Else,
        else
        {
            // a. Let n be +0𝔽.
            n = 0;
        }

        // 3. If NewTarget is undefined, return n.
        if (newTarget.IsUndefined()) return n;

        // FIXME: 4. Let O be ? OrdinaryCreateFromConstructor(NewTarget, "%Number.prototype%", « [[NumberData]] »).
        // 5. Set O.[[NumberData]] to n.
        var O = new NumberObject(vm.ObjectPrototype, n);

        // 6. Return O.
        return O;
    }
}
