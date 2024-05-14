using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 20.2.3 Properties of the Function Prototype Object, https://tc39.es/ecma262/#sec-properties-of-the-function-prototype-object
internal sealed class FunctionPrototype : Object
{
    // The Function prototype has a [[Prototype]] internal slot whose value is %Object.prototype%.
    public FunctionPrototype(ObjectPrototype objectPrototype) : base(objectPrototype)
    {
    }

    public void Initialize(VM vm)
    {
        // 20.2.3.3 Function.prototype.call ( thisArg, ...args ), https://tc39.es/ecma262/#sec-function.prototype.call
        var callBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, call, 1, "call");
        DataProperties.Add("call", new Property(callBuiltin, new(true, false, true)));
    }

    // 20.2.3.3 Function.prototype.call ( thisArg, ...args ), https://tc39.es/ecma262/#sec-function.prototype.call
    private Completion call(VM vm, Value thisArg, List argumentList)
    {
        // 1. Let func be the this value.
        var func = thisArg;

        // 2. If IsCallable(func) is false, throw a TypeError exception.
        if (!func.IsCallable()) return ThrowTypeError(vm, RuntimeErrorType.CallingANonFunction, func?.Type() ?? ValueType.Undefined);

        // FIXME: 3. Perform PrepareForTailCall().

        // 4. Return ? Call(func, thisArg, args).
        var newThisArg = argumentList[0];
        var args = new List(argumentList.Values.Skip(1));
        return Call(vm, func, newThisArg, args);
    }
}
