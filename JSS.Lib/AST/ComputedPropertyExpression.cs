using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.3.2 Property Accessors, https://tc39.es/ecma262/#sec-property-accessors
internal sealed class ComputedPropertyExpression : IExpression
{
    public ComputedPropertyExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // 13.3.2.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-property-accessors-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let baseReference be ? Evaluation of MemberExpression.
        var baseReference = Lhs.Evaluate(vm);
        if (baseReference.IsAbruptCompletion()) return baseReference;

        // 2. Let baseValue be ? GetValue(baseReference).
        var baseValue = baseReference.Value.GetValue(vm);
        if (baseValue.IsAbruptCompletion()) return baseValue;

        // 3. If the source text matched by this MemberExpression is strict mode code, let strict be true; else let strict be false.

        // 4. Return ? EvaluatePropertyAccessWithExpressionKey(baseValue, Expression, strict).
        return EvaluatePropertyAccessWithExpressionKey(vm, baseValue.Value);
    }

    // 13.3.3 EvaluatePropertyAccessWithExpressionKey ( baseValue, expression, strict ), https://tc39.es/ecma262/#sec-evaluate-property-access-with-expression-key
    private Completion EvaluatePropertyAccessWithExpressionKey(VM vm, Value baseValue)
    {
        // 1. Let propertyNameReference be ? Evaluation of expression.
        var propertyNameReference = Rhs.Evaluate(vm);
        if (propertyNameReference.IsAbruptCompletion()) return propertyNameReference;

        // 2. Let propertyNameValue be ? GetValue(propertyNameReference).
        var propertyNameValue = propertyNameReference.Value.GetValue(vm);
        if (propertyNameValue.IsAbruptCompletion()) return propertyNameValue;

        // 3. Let propertyKey be ? ToPropertyKey(propertyNameValue).
        var propertyKey = propertyNameValue.Value.ToPropertyKey();
        if (propertyKey.IsAbruptCompletion()) return propertyKey;

        // 4. Return the Reference Record { [[Base]]: baseValue, [[ReferencedName]]: propertyKey, [[Strict]]: strict, [[ThisValue]]: EMPTY }.
        var propertyString = propertyKey.Value.AsString();
        return Reference.Resolvable(baseValue, propertyString.Value, Empty.The);
    }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
