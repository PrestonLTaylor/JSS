﻿namespace JSS.Lib.AST;

// 13.11 Equality Operators, https://tc39.es/ecma262/#sec-equality-operators
internal sealed class LooseEqualityExpression : IExpression
{
    public LooseEqualityExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.11.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-equality-operators-runtime-semantics-evaluation
    public void Execute() { }

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
