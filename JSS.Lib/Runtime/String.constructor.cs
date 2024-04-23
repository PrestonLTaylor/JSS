using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

internal class StringConstructor : Object, ICallable, IConstructable
{
    // The String constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public StringConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    // 22.1.1.1 String ( value ), https://tc39.es/ecma262/#sec-string-constructor-string-value
    public Completion Call(VM vm, Value thisArgument, List argumentList)
    {
        // 1. If value is not present, then
        string s;
        if (argumentList.Values.Count == 0)
        {
            s = "";
        }
        else
        {
            // FIXME: a. If NewTarget is undefined and value is a Symbol, return SymbolDescriptiveString(value).

            // b. Let s be ? ToString(value).
            var value = argumentList.Values[0];
            var abruptOrS = value.ToStringJS(vm);
            if (abruptOrS.IsAbruptCompletion()) return abruptOrS.Completion;
            s = abruptOrS.Value;
        }

        // NOTE/FIXME: I think this is always true for calls
        // 3. If NewTarget is undefined, return s.
        return s; 
    }

    // 22.1.1.1 String ( value ), https://tc39.es/ecma262/#sec-string-constructor-string-value
    public Completion Construct(VM vm, List argumentsList)
    {
        throw new NotImplementedException();
    }
}
