namespace JSS.Lib.AST;

// 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
internal sealed class IfStatement : INode
{
    public IfStatement(IExpression ifExpression, INode ifCaseStatement, INode? elseCaseStatement)
    {
        IfExpression = ifExpression;
        IfCaseStatement = ifCaseStatement;
        ElseCaseStatement = elseCaseStatement;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Let names1 be VarDeclaredNames of the first Statement.
        var names = IfCaseStatement.VarDeclaredNames();

        // 2. Let names2 be VarDeclaredNames of the second Statement.
        if (ElseCaseStatement is not null)
        {
            names.AddRange(ElseCaseStatement.VarDeclaredNames());
        }

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // FIXME: 14.6.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-if-statement-runtime-semantics-evaluation

    public IExpression IfExpression { get; }
    public INode IfCaseStatement { get; }
    public INode? ElseCaseStatement { get; }
}
