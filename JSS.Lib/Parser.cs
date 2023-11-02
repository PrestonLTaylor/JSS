﻿using JSS.Lib.AST;
using JSS.Lib.AST.Literal;

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
        if (IsIdentifier())
        {
            return ParseIdentifier();
        }
        if (IsNullLiteral())
        {
            return ParseNullLiteral();
        }
        if (IsBooleanLiteral())
        {
            return ParseBooleanLiteral();
        }
        if (IsNumericLiteral())
        {
            return ParseNumericLiteral();
        }
        if (IsStringLiteral())
        {
            return ParseStringLiteral();
        }
        if (IsBlock())
        {
            return ParseBlock();
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
        return WrapExpression(new ThisExpression());
    }

    // 13.2.2 Identifier Reference, https://tc39.es/ecma262/#sec-identifier-reference
    private bool IsIdentifier()
    {
        return _consumer.IsTokenOfType(TokenType.Identifier);
    }

    private ExpressionStatement ParseIdentifier()
    {
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);
        return WrapExpression(new Identifier(identifierToken.data));
    }

    // 13.2.3 Literals, https://tc39.es/ecma262/#sec-primary-expression-literals
    private bool IsNullLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.Null);
    }

    private ExpressionStatement ParseNullLiteral()
    {
        _consumer.ConsumeTokenOfType(TokenType.Null);
        return WrapExpression(new NullLiteral());
    }

    private bool IsBooleanLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.False) || _consumer.IsTokenOfType(TokenType.True);
    }

    private ExpressionStatement ParseBooleanLiteral()
    {
        // FIXME: This doesn't have an explicit assertion
        var booleanToken = _consumer.Consume();
        var booleanValue = booleanToken.type == TokenType.True;
        return WrapExpression(new BooleanLiteral(booleanValue));
    }

    private bool IsNumericLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.Number);
    }

    private ExpressionStatement ParseNumericLiteral()
    {
        var numericToken = _consumer.ConsumeTokenOfType(TokenType.Number);
        // FIXME: Proper error reporting
        // FIXME: Parse numbers according to the JS spec rather than the C# parse library
        var numericValue = double.Parse(numericToken.data);
        return WrapExpression(new NumericLiteral(numericValue));
    }

    private bool IsStringLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.String);
    }

    private ExpressionStatement ParseStringLiteral()
    {
        var stringLiteral = _consumer.ConsumeTokenOfType(TokenType.String);
        var stringValue = stringLiteral.data[1..^1];
        return WrapExpression(new StringLiteral(stringValue));
    }

    // 14.2 Block, https://tc39.es/ecma262/#sec-block
    private bool IsBlock()
    {
        return _consumer.IsTokenOfType(TokenType.OpenBrace);
    }

    private Block ParseBlock()
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        List<INode> blockNodes = new();
        while (!_consumer.IsTokenOfType(TokenType.ClosedBrace))
        {
            throw new NotImplementedException();
        }

        // FIXME: Throw a SyntaxError if we encounter a Block without a closed brace
        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new Block(blockNodes);
    }

    private ExpressionStatement WrapExpression(IExpression expression)
    {
        return new ExpressionStatement(expression);
    }
}
