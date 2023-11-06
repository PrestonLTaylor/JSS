using JSS.Lib.AST;
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
        if (TryParseExpression(out IExpression? parsedExpression))
        {
            return new ExpressionStatement(parsedExpression!);
        }
        if (IsBlock())
        {
            return ParseBlock();
        }
        if (IsLetDeclaration())
        {
            return ParseLetDeclaration();
        }
        if (IsConstDeclaration())
        {
            return ParseConstDeclaration();
        }
        if (IsVarStatement())
        {
            return ParseVarStatement();
        }
        if (IsEmptyStatement())
        {
            return ParseEmptyStatement();
        }
        if (IsIfStatement())
        {
            return ParseIfStatement();
        }
        if (IsReturnStatement())
        {
            return ParseReturnStatement();
        }
        if (IsContinueStatement())
        {
            return ParseContinueStatement();
        }
        if (IsThrowStatement())
        {
            return ParseThrowStatement();
        }
        if (IsTryStatement())
        {
            return ParseTryStatement();
        }

        throw new NotImplementedException();
    }

    private bool TryParseExpression(out IExpression? parsedExpression)
    {
        if (IsThisExpression())
        {
            parsedExpression = ParseThisExpression();
            return true;
        }
        if (IsIdentifier())
        {
            parsedExpression = ParseIdentifier();
            return true;
        }
        if (IsNullLiteral())
        {
            parsedExpression = ParseNullLiteral();
            return true;
        }
        if (IsBooleanLiteral())
        {
            parsedExpression = ParseBooleanLiteral();
            return true;
        }
        if (IsNumericLiteral())
        {
            parsedExpression = ParseNumericLiteral();
            return true;
        }
        if (IsStringLiteral())
        {
            parsedExpression = ParseStringLiteral();
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // 13.2.1 The this Keyword, https://tc39.es/ecma262/#sec-this-keyword
    private bool IsThisExpression()
    {
        return _consumer.IsTokenOfType(TokenType.This);
    }

    private ThisExpression ParseThisExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.This);
        return new ThisExpression();
    }

    // 13.2.2 Identifier Reference, https://tc39.es/ecma262/#sec-identifier-reference
    private bool IsIdentifier()
    {
        return _consumer.IsTokenOfType(TokenType.Identifier);
    }

    private Identifier ParseIdentifier()
    {
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);
        return new Identifier(identifierToken.data);
    }

    // 13.2.3 Literals, https://tc39.es/ecma262/#sec-primary-expression-literals
    private bool IsNullLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.Null);
    }

    private NullLiteral ParseNullLiteral()
    {
        _consumer.ConsumeTokenOfType(TokenType.Null);
        return new NullLiteral();
    }

    private bool IsBooleanLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.False) || _consumer.IsTokenOfType(TokenType.True);
    }

    private BooleanLiteral ParseBooleanLiteral()
    {
        // FIXME: This doesn't have an explicit assertion
        var booleanToken = _consumer.Consume();
        var booleanValue = booleanToken.type == TokenType.True;
        return new BooleanLiteral(booleanValue);
    }

    private bool IsNumericLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.Number);
    }

    private NumericLiteral ParseNumericLiteral()
    {
        var numericToken = _consumer.ConsumeTokenOfType(TokenType.Number);
        // FIXME: Proper error reporting
        // FIXME: Parse numbers according to the JS spec rather than the C# parse library
        var numericValue = double.Parse(numericToken.data);
        return new NumericLiteral(numericValue);
    }

    private bool IsStringLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.String);
    }

    private StringLiteral ParseStringLiteral()
    {
        var stringLiteral = _consumer.ConsumeTokenOfType(TokenType.String);
        var stringValue = stringLiteral.data[1..^1];
        return new StringLiteral(stringValue);
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

    // 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
    private bool IsLetDeclaration()
    {
        return _consumer.IsTokenOfType(TokenType.Let);
    }

    private LetDeclaration ParseLetDeclaration()
    {
        _consumer.ConsumeTokenOfType(TokenType.Let);

        // FIXME: If let is being used as an identifier or as a var declaration then we parse it as such instead of throwing
        // FIXME: Allow await/yield to be used as an indentifier when specified in the spec
        // FIXME: Throw a SyntaxError instead of throwing (also have a specific error if the next token is let as specified in the spec)
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single let declaration
        INode? initializer = null;
        if (_consumer.IsTokenOfType(TokenType.Assignment))
        {
            _consumer.ConsumeTokenOfType(TokenType.Assignment);
            initializer = ParseInitializer();
        }

        return new LetDeclaration(identifierToken.data, initializer);
    }

    private bool IsConstDeclaration()
    {
        return _consumer.IsTokenOfType(TokenType.Const);
    }

    private ConstDeclaration ParseConstDeclaration()
    {
        _consumer.ConsumeTokenOfType(TokenType.Const);

        // FIXME: Allow await/yield to be used as an indentifier when specified in the spec
        // FIXME: Throw a SyntaxError instead of throwing (also have a specific error if the next token is let as specified in the spec)
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single const declaration
        if (!_consumer.IsTokenOfType(TokenType.Assignment))
        {
            // FIXME: Throw a SyntaxError
            throw new InvalidOperationException();
        }

        _consumer.ConsumeTokenOfType(TokenType.Assignment);
        var initializer = ParseInitializer();

        return new ConstDeclaration(identifierToken.data, initializer);
    }

    // 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
    private bool IsVarStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Var);
    }

    private VarStatement ParseVarStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Var);

        // FIXME: Allow let/await/yield to be used as an indentifier when specified in the spec
        // FIXME: Throw a SyntaxError instead of throwing
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single var statement 
        INode? initializer = null;
        if (_consumer.IsTokenOfType(TokenType.Assignment))
        {
            _consumer.ConsumeTokenOfType(TokenType.Assignment);
            initializer = ParseInitializer();
        }

        return new VarStatement(identifierToken.data, initializer);
    }

    private INode ParseInitializer()
    {
        // FIXME: Parse according to the spec: https://tc39.es/ecma262/#prod-Initializer
        if (TryParseExpression(out IExpression? parsedExpression))
        {
            return parsedExpression!;
        }

        // FIXME: Throw a SyntaxError instead
        throw new InvalidOperationException();
    }

    // 14.4 Empty Statement, https://tc39.es/ecma262/#sec-empty-statement
    private bool IsEmptyStatement()
    {
        return _consumer.IsTokenOfType(TokenType.SemiColon);
    }

    private EmptyStatement ParseEmptyStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.SemiColon);
        // FIXME: Have a "global" EmptyStatement, so we don't have multiple redunant empty statements
        return new EmptyStatement();
    }

    // 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
    private bool IsIfStatement()
    {
        return _consumer.IsTokenOfType(TokenType.If);
    }

    // FIXME: Implement 14.6.1 Static Semantics: Early Errors, https://tc39.es/ecma262/#sec-if-statement-static-semantics-early-errors
    private IfStatement ParseIfStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.If);
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        IExpression? ifExpression;
        if (!TryParseExpression(out ifExpression))
        {
            // FIXME: Throw a SyntaxError
            throw new InvalidOperationException();
        }

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        var ifCaseStatement = ParseStatement();
        TryParseElseStatement(out INode? elseCaseStatement);

        return new IfStatement(ifExpression!, ifCaseStatement, elseCaseStatement);
    }

    private bool TryParseElseStatement(out INode? elseCaseStatement)
    {
        if (!_consumer.IsTokenOfType(TokenType.Else))
        {
            elseCaseStatement = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.Else);

        elseCaseStatement = ParseStatement();
        return true;
    }

    // 14.8 The continue Statement, https://tc39.es/ecma262/#prod-ContinueStatement
    private bool IsContinueStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Continue);
    }

    private ContinueStatement ParseContinueStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Continue);

        Identifier? label = null;
        if (IsIdentifier())
        {
            label = ParseIdentifier();
        }

        return new ContinueStatement(label);
    }

    // 14.10 The return Statement, https://tc39.es/ecma262/#sec-return-statement
    private bool IsReturnStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Return);
    }

    private ReturnStatement ParseReturnStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Return);

        // FIXME: return [no LineTerminator here] Expression[+In, ?Yield, ?Await] ;
        // Don't parse an expression if there is a line terminator after the return
        TryParseExpression(out IExpression? returnExpression);
        return new ReturnStatement(returnExpression);
    }

    // 14.14 The throw Statement, https://tc39.es/ecma262/#sec-throw-statement
    private bool IsThrowStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Throw);
    }

    private ThrowStatement ParseThrowStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Throw);

        // FIXME: throw [no LineTerminator here] Expression[+In, ?Yield, ?Await] ;
        // Don't parse an expression if there is a line terminator after the return
        if (TryParseExpression(out IExpression? throwExpression))
        {
            return new ThrowStatement(throwExpression!);
        }
        else
        {
            // FIXME: Throw a SyntaxError
            throw new InvalidOperationException();
        }
    }

    // 14.15 The try Statement, https://tc39.es/ecma262/#sec-try-statement
    private bool IsTryStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Try);
    }

    private TryStatement ParseTryStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Try);

        var tryBlock = ParseBlock();

        var didParseCatch = TryParseCatchBlock(out Block? catchBlock, out Identifier? catchParameter);
        var didParseFinally = TryParseFinallyBlock(out Block? finallyBlock);

        if (!didParseCatch && !didParseFinally)
        {
            // FIXME: Throw SyntaxError
            throw new InvalidOperationException();
        }

        return new TryStatement(tryBlock, catchBlock, catchParameter, finallyBlock);
    }

    private bool TryParseCatchBlock(out Block? catchBlock, out Identifier? catchParameter)
    {
        if (_consumer.IsTokenOfType(TokenType.Catch))
        {
            ParseCatchBlock(out catchBlock, out catchParameter);
            return true;
        }

        catchBlock = null;
        catchParameter = null;
        return false;
    }

    private void ParseCatchBlock(out Block catchBlock, out Identifier? catchParameter)
    {
        _consumer.ConsumeTokenOfType(TokenType.Catch);

        TryParseCatchParameter(out catchParameter);

        catchBlock = ParseBlock();
    }

    private bool TryParseCatchParameter(out Identifier? catchParameter)
    {
        if (!_consumer.IsTokenOfType(TokenType.OpenParen))
        {
            catchParameter = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        catchParameter = ParseIdentifier();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        return true;
    }

    private bool TryParseFinallyBlock(out Block? finallyBlock)
    {
        if (!_consumer.IsTokenOfType(TokenType.Finally))
        {
            finallyBlock = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.Finally);
        finallyBlock = ParseBlock();
        return true;
    }
}
