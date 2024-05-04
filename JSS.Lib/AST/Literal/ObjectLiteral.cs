﻿using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

// 13.2.5 Object Initializer, https://tc39.es/ecma262/#sec-object-initializer
internal class ObjectLiteral : IExpression
{
    public ObjectLiteral(List<INode> propertyDefinitionList)
    {
        PropertyDefinitions = propertyDefinitionList;
    }

    // 13.2.5.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-object-initializer-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // FIXME: Implement the rest of the evaluation when we parse property definitions
        // 1. Return OrdinaryObjectCreate(%Object.prototype%).
        return new Object(vm.ObjectPrototype);
    }

    public IReadOnlyList<INode> PropertyDefinitions { get; }
}
