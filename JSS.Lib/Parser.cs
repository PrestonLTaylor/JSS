using JSS.Lib.AST;

namespace JSS.Lib;

internal sealed class Parser
{
    private readonly TokenConsumer _consumer;

    public Parser(string toParse)
    {
        var lexer = new Lexer(toParse);
        _consumer = new TokenConsumer(lexer.Lex().ToList());
    }

    public Program Parse()
    {
        List<INode> nodes = new();

        while (_consumer.CanConsume())
        {
            nodes.Add(ParseStatement());
        }

        return new Program(nodes);
    }

    private INode ParseStatement()
    {
        if (IsThisExpression())
        {
            return ParseThisExpression();
        }

        throw new NotImplementedException();
    }

    // 13.2.1 The this Keyword, https://tc39.es/ecma262/#sec-this-keyword
    private bool IsThisExpression()
    {
        return _consumer.IsTokenOfType(TokenType.This);
    }

    private ExpressionStatement ParseThisExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.This);
        return new ExpressionStatement(new ThisExpression());
    }
}
