﻿using JSS.Lib.Execution;
using System;

namespace JSS.Lib.AST;

// 14.2 Block, https://tc39.es/ecma262/#sec-block
internal sealed class Block : INode 
{
    public Block(StatementList statements)
    {
        Statements = statements;
    }

    // 14.2.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // FIXME: 1. Let oldEnv be the running execution context's LexicalEnvironment.
        // FIXME: 2. Let blockEnv be NewDeclarativeEnvironment(oldEnv).
        // FIXME: 3. Perform BlockDeclarationInstantiation(StatementList, blockEnv).
        // FIXME: 4. Set the running execution context's LexicalEnvironment to blockEnv.

        // 5. Let blockValue be Completion(Evaluation of StatementList).
        var blockValue = Statements.Evaluate(vm);

        // FIXME: 6. Set the running execution context's LexicalEnvironment to oldEnv.

        // 7. Return ? blockValue.
        return blockValue;
    }

    public StatementList Statements { get; }
}
