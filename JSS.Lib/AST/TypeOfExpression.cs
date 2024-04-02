using JSS.Lib.Execution;
using JSS.Lib.AST.Values;
using String = JSS.Lib.AST.Values.String;
using Boolean = JSS.Lib.AST.Values.Boolean;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.5.3 The typeof Operator, https://tc39.es/ecma262/#sec-typeof-operator
internal sealed class TypeOfExpression : IExpression
{
    public TypeOfExpression(IExpression expression)
    {
        Expression = expression;
    }

    // 13.5.3.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-typeof-operator-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Let val be ? Evaluation of UnaryExpression.
        var val = Expression.Evaluate(vm);
        if (val.IsAbruptCompletion()) return val;

        // 2. If val is a Reference Record, then
        if (val.Value.IsReference())
        {
            // a. If IsUnresolvableReference(val) is true, return "undefined".
            var asReference = val.Value.AsReference();
            if (asReference.IsUnresolvableReference())
            {
                return new String("undefined");
            }
        }

        // 3. Set val to ? GetValue(val).
        val = val.Value.GetValue();
        if (val.IsAbruptCompletion()) return val;

        // 4. If val is undefined, return "undefined".
        if (val.Value.IsUndefined())
        {
            return new String("undefined");
        }

        // 5. If val is null, return "object".
        if (val.Value.IsNull())
        {
            return new String("object");
        }

        // 6. If val is a String, return "string".
        if (val.Value.IsString())
        {
            return new String("string");
        }

        // 7. If val is a Symbol, return "symbol".
        if (val.Value.IsSymbol())
        {
            return new String("symbol");
        }

        // 8. If val is a Boolean, return "boolean".
        if (val.Value.IsBoolean())
        {
            return new String("boolean");
        }

        // 9. If val is a Number, return "number".
        if (val.Value.IsNumber())
        {
            return new String("number");
        }

        // 10. If val is a BigInt, return "bigint".
        if (val.Value.IsBigInt())
        {
            return new String("bigint");
        }

        // 11. Assert: val is an Object.
        Debug.Assert(val.Value.IsObject());

        // 12. NOTE: This step is replaced in section B.3.6.3.

        // 13. If val has a [[Call]] internal slot, return "function".
        if (val.Value.HasInternalCall())
        {
            return new String("function");
        }

        // 14. Return "object".
        return new String("object");
    }

    public IExpression Expression { get; }
}
