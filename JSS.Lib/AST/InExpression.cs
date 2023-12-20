using JSS.Lib.Execution;
using Object = JSS.Lib.AST.Values.Object;
using String = JSS.Lib.AST.Values.String;

namespace JSS.Lib.AST;

// 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
internal sealed class InExpression : IExpression
{
    public InExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-relational-operators-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let lref be ? Evaluation of RelationalExpression.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 2. Let lval be ? GetValue(lref).
        var lval = lref.Value.GetValue();
        if (lval.IsAbruptCompletion()) return lval;

        // 3. Let rref be ? Evaluation of ShiftExpression.
        var rref = Rhs.Evaluate(vm);
        if (rref.IsAbruptCompletion()) return rref;

        // 4. Let rval be ? GetValue(rref).
        var rval = rref.Value.GetValue();
        if (rval.IsAbruptCompletion()) return rval;

        // 5. If rval is not an Object, FIXME: throw a TypeError exception.
        if (!rval.Value.IsObject())
        {
            return Completion.ThrowCompletion(new String($"rhs of 'in' should be an Object, but got {rval.Value.Type()}"));
        }

        // 6. Return ? HasProperty(rval, ? ToPropertyKey(lval)).
        var asObject = (rval.Value as Object)!;

        var propertyKey = lval.Value.ToPropertyKey();
        if (propertyKey.IsAbruptCompletion()) return propertyKey;

        // FIXME: We should be able to handle Symbols
        var propertyKeyAsString = (propertyKey.Value as String)!;

        return Object.HasProperty(asObject, propertyKeyAsString.Value);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
