using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

// 13.2.5 Object Initializer, https://tc39.es/ecma262/#sec-object-initializer
internal class ObjectLiteral : IExpression
{
    public ObjectLiteral(List<IPropertyDefinition> propertyDefinitionList)
    {
        PropertyDefinitions = propertyDefinitionList;
    }

    // 13.2.5.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-object-initializer-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let obj be OrdinaryObjectCreate(%Object.prototype%).
        var obj = new Object(vm.ObjectPrototype);

        // 2. Perform ? PropertyDefinitionEvaluation of PropertyDefinitionList with argument obj.
        var definitionResult = PropertyDefinitionEvaluation(vm, obj);
        if (definitionResult.IsAbruptCompletion()) return definitionResult;

        // 3. Return obj.
        return obj;
    }

    // 13.2.5.5 Runtime Semantics: PropertyDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-propertydefinitionevaluation
    private Completion PropertyDefinitionEvaluation(VM vm, Object obj)
    {
        // 1. Perform ? PropertyDefinitionEvaluation of PropertyDefinitionList with argument object.
        // 2. Perform ? PropertyDefinitionEvaluation of PropertyDefinition with argument object.
        foreach (var propertyDefinition in PropertyDefinitions)
        {
            var definitionResult = propertyDefinition.PropertyDefinitionEvaluation(vm, obj);
            if (definitionResult.IsAbruptCompletion()) return definitionResult;
        }

        // 3. Return UNUSED.
        return Empty.The;
    }


    public IReadOnlyList<IPropertyDefinition> PropertyDefinitions { get; }
}
