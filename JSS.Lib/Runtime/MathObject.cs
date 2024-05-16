using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 21.3 The Math Object, https://tc39.es/ecma262/#sec-math-object
internal sealed class MathObject : Object
{
    // The Math object has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public MathObject(ObjectPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(VM vm)
    {
        // 21.3.2.26 Math.pow ( base, exponent ), https://tc39.es/ecma262/#sec-math.pow
        var powBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, pow, 2, "pow");
        DataProperties.Add("pow", new(powBuiltin, new(true, false, true)));
    }

    // 21.3.2.26 Math.pow ( base, exponent ), https://tc39.es/ecma262/#sec-math.pow
    private Completion pow(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Set base to ? ToNumber(base).
        var powBase = argumentList[0].ToNumber(vm);
        if (powBase.IsAbruptCompletion()) return powBase.Completion;

        // 2. Set exponent to ? ToNumber(exponent).
        var exponent = argumentList[1].ToNumber(vm);
        if (exponent.IsAbruptCompletion()) return exponent.Completion;

        // 3. Return Number::exponentiate(base, exponent).
        return Number.Exponentiate(vm, powBase.Value, exponent.Value);
    }
}
