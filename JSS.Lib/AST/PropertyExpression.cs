using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 13.3.2 Property Accessors, https://tc39.es/ecma262/#sec-property-accessors
internal sealed class PropertyExpression : IExpression
{
    public PropertyExpression(IExpression lhs, string rhs)
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

        // FIXME: 3. If the source text matched by this MemberExpression is strict mode code, let strict be true; else let strict be false.

        // 4. Return EvaluatePropertyAccessWithIdentifierKey(baseValue, IdentifierName, strict).
        return EvaluatePropertyAccessWithIdentifierKey(baseValue.Value);
    }

    // 13.3.4 EvaluatePropertyAccessWithIdentifierKey, https://tc39.es/ecma262/#sec-evaluate-property-access-with-identifier-key
    private Reference EvaluatePropertyAccessWithIdentifierKey(Value baseValue)
    {
        // 1. Let propertyNameString be StringValue of identifierName.

        // 2. Return the Reference Record { [[Base]]: baseValue, [[ReferencedName]]: propertyNameString, [[Strict]]: strict, [[ThisValue]]: EMPTY }.
        return Reference.Resolvable(baseValue, Rhs, Empty.The);
    }

    public IExpression Lhs { get; }
    public string Rhs { get; }
}
