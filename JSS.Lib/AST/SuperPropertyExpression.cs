namespace JSS.Lib.AST;

// 13.3.7 The super Keyword, https://tc39.es/ecma262/#sec-super-keyword
internal sealed class SuperPropertyExpression : IExpression
{
    public SuperPropertyExpression(string name)
    {
        Name = name;
    }

    // FIXME: 13.3.7.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-super-keyword-runtime-semantics-evaluation
    public void Execute() { }

    public string Name { get; }
}
