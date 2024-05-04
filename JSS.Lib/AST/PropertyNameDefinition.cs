using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal sealed class PropertyNameDefinition : IPropertyDefinition
{
    public PropertyNameDefinition(INode propertyName, IExpression expression)
    {
        PropertyName = propertyName;
        Expression = expression;
    }

    // 13.2.5.5 Runtime Semantics: PropertyDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-propertydefinitionevaluation
    public Completion PropertyDefinitionEvaluation(VM vm, Object obj)
    {
        // 1. Let propKey be ? Evaluation of PropertyName.
        var propKey = PropertyName.Evaluate(vm);
        if (propKey.IsAbruptCompletion()) return propKey;

        // FIXME: 2. If this PropertyDefinition is contained within a Script that is being evaluated for JSON.parse (see step 7 of JSON.parse), then
        // FIXME: a. Let isProtoSetter be false.
        // FIXME: 3. Else if propKey is "__proto__" and IsComputedPropertyKey of PropertyName is false, then
        // FIXME: a. Let isProtoSetter be true.
        // FIXME: 4. Else,
        // FIXME: a. Let isProtoSetter be false.
        // FIXME: 5. If IsAnonymousFunctionDefinition(AssignmentExpression) is true and isProtoSetter is false, then
        // FIXME: a. Let propValue be ? NamedEvaluation of AssignmentExpression with argument propKey.
        // 6. Else,
        // a. Let exprValueRef be ? Evaluation of AssignmentExpression.
        var exprValueRef = Expression.Evaluate(vm);
        if (exprValueRef.IsAbruptCompletion()) return exprValueRef;

        // b. Let propValue be ? GetValue(exprValueRef).
        var propValue = exprValueRef.Value.GetValue(vm);
        if (propValue.IsAbruptCompletion()) return propValue;

        // FIXME: 7. If isProtoSetter is true, then
        // FIXME: a. If propValue is an Object or propValue is null, then
        // FIXME: i. Perform ! object.[[SetPrototypeOf]](propValue).
        // FIXME: b. Return UNUSED.
        // FIXME: 8. Assert: object is an ordinary, extensible object with no non-configurable properties.

        // 9. Perform ! CreateDataPropertyOrThrow(object, propKey, propValue).
        MUST(Object.CreateDataPropertyOrThrow(vm, obj, propKey.Value.AsString(), propValue.Value));

        // 10. Return UNUSED.
        return Empty.The;
    }

    public INode PropertyName { get; }
    public IExpression Expression { get; }
}
