﻿namespace JSS.Lib.AST;

// 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
internal sealed class LessThanEqualsExpression : IExpression
{
    public LessThanEqualsExpression(IExpression lhs, IExpression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    // FIXME: 13.10.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-relational-operators-runtime-semantics-evaluation

    public IExpression Lhs { get; }
    public IExpression Rhs { get; }
}
