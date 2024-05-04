using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// Represents the definition of a property that has the "PropertyDefinitionEvaluation" semantics, https://tc39.es/ecma262/#sec-runtime-semantics-propertydefinitionevaluation
internal interface IPropertyDefinition
{
    // 13.2.5.5 Runtime Semantics: PropertyDefinitionEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-propertydefinitionevaluation
    public Completion PropertyDefinitionEvaluation(VM vm, Object obj);
}
