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
        return new Program(ParseScript());
    }

    // 16.1 Scripts, https://tc39.es/ecma262/#sec-scripts
    private List<INode> ParseScript()
    {
        return ParseStatementListWhile(() => _consumer.CanConsume());
    }

    // StatementList, https://tc39.es/ecma262/#prod-StatementList
    private List<INode> ParseStatementListWhile(Func<bool> shouldParse)
    {
        List<INode> nodes = new();

        while (shouldParse())
        {
            nodes.Add(ParseStatementListItem());
        }

        return nodes;
    }

    // StatementListItem, https://tc39.es/ecma262/#prod-StatementListItem
    private INode ParseStatementListItem()
    {
        if (TryParseStatement(out INode? statement))
        {
            return statement!;
        }
        if (TryParseDeclaration(out INode? declaration))
        {
            return declaration!;
        }

        throw new NotImplementedException();
    }

    // Statement, https://tc39.es/ecma262/#prod-Statement
    private INode ParseStatement()
    {
        if (TryParseStatement(out INode? statement))
        {
            return statement!;
        }

        // FIXME: Change to SyntaxError when we have full statement parsing
        throw new NotImplementedException();
    }

    private bool TryParseStatement(out INode? statement)
    {
        if (IsBlock())
        {
            statement = ParseBlock();
            return true;
        }
        if (IsVarStatement())
        {
            statement = ParseVarStatement();
            return true;
        }
        if (IsEmptyStatement())
        {
            statement = ParseEmptyStatement();
            return true;
        }
        if (TryParseExpression(out IExpression? parsedExpression))
        {
            statement = new ExpressionStatement(parsedExpression!);
            return true;
        }
        if (IsIfStatement())
        {
            statement = ParseIfStatement();
            return true;
        }
        if (IsDoWhileStatement())
        {
            statement = ParseDoWhileStatement();
            return true;
        }
        if (IsWhileStatement())
        {
            statement = ParseWhileStatement();
            return true;
        }
        if (IsForStatement())
        {
            statement = ParseForStatement();
            return true;
        }
        if (IsBreakStatement())
        {
            statement = ParseBreakStatement();
            return true;
        }
        if (IsContinueStatement())
        {
            statement = ParseContinueStatement();
            return true;
        }
        if (IsReturnStatement())
        {
            statement = ParseReturnStatement();
            return true;
        }
        if (IsThrowStatement())
        {
            statement = ParseThrowStatement();
            return true;
        }
        if (IsTryStatement())
        {
            statement = ParseTryStatement();
            return true;
        }
        if (IsDebuggerStatement())
        {
            statement = ParseDebuggerStatement();
            return true;
        }

        statement = null;
        return false;
    }

    // Declaration, https://tc39.es/ecma262/#prod-Declaration
    private bool TryParseDeclaration(out INode? declaration)
    {
        if (IsLetDeclaration())
        {
            declaration = ParseLetDeclaration();
            return true;
        }
        if (IsConstDeclaration())
        {
            declaration = ParseConstDeclaration();
            return true;
        }
        if (IsFunctionDeclaration())
        {
            declaration = ParseFunctionDeclaration();
            return true;
        }
        if (IsClassDeclaration())
        {
            declaration = ParseClassDeclaration();
            return true;
        }

        declaration = null;
        return false;
    }

    // Expression, https://tc39.es/ecma262/#prod-Expression
    private bool TryParseExpression(out IExpression? parsedExpression)
    {
        // FIXME: Implement the comma operator
        if (TryParseAssignmentExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // AssignmentExpression, https://tc39.es/ecma262/#prod-AssignmentExpression
    private bool TryParseAssignmentExpression(out IExpression? parsedExpression)
    {
        if (TryParseConditionalExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // ConditionalExpression, https://tc39.es/ecma262/#prod-ConditionalExpression
    private bool TryParseConditionalExpression(out IExpression? parsedExpression)
    {
        // FIXME: Implement the '?' operator
        if (TryParseShortCircuitExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // ShortCircuitExpression, https://tc39.es/ecma262/#prod-ShortCircuitExpression
    private bool TryParseShortCircuitExpression(out IExpression? parsedExpression)
    {
        // FIXME: Implement the coalesing operator
        if (TryParseLogicalOrExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // LogicalORExpression, https://tc39.es/ecma262/#prod-LogicalORExpression
    private bool TryParseLogicalOrExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseLogicalAndExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an ||, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!_consumer.IsTokenOfType(TokenType.Or))
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.Or);

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = new LogicalOrExpression(lhs!, rhs!);
        return true;
    }

    // LogicalANDExpression, https://tc39.es/ecma262/#prod-LogicalANDExpression
    private bool TryParseLogicalAndExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseBitwiseORExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an &&, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!_consumer.IsTokenOfType(TokenType.And))
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.And);

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = new LogicalAndExpression(lhs!, rhs!);
        return true;
    }

    // BitwiseORExpression, https://tc39.es/ecma262/#prod-BitwiseORExpression
    private bool TryParseBitwiseORExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseBitwiseXORExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an |, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!_consumer.IsTokenOfType(TokenType.BitwiseOr))
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseOr);

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = new BitwiseOrExpression(lhs!, rhs!);
        return true;
    }

    // BitwiseXORExpression, https://tc39.es/ecma262/#prod-BitwiseXORExpression
    private bool TryParseBitwiseXORExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseBitwiseAndExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an |, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!_consumer.IsTokenOfType(TokenType.BitwiseXor))
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseXor);

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = new BitwiseXorExpression(lhs!, rhs!);
        return true;
    }

    // BitwiseANDExpression, https://tc39.es/ecma262/#prod-BitwiseANDExpression
    private bool TryParseBitwiseAndExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseEqualityExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an |, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!_consumer.IsTokenOfType(TokenType.BitwiseAnd))
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseAnd);

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = new BitwiseAndExpression(lhs!, rhs!);
        return true;
    }

    // 13.11 Equality Operators, https://tc39.es/ecma262/#sec-equality-operators
    private bool TryParseEqualityExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseRelationalExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have an equality operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsEqualityOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var equalityToken = _consumer.Consume();

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = CreateEqualityExpression(lhs!, rhs!, equalityToken);
        return true;
    }

    private bool IsEqualityOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.EqualEquals or TokenType.StrictEqualsEquals or TokenType.NotEquals or TokenType.StrictNotEquals => true,
            _ => false,
        };
    }

    private IExpression CreateEqualityExpression(IExpression lhs, IExpression rhs, Token equalityToken)
    {
        return equalityToken.type switch
        {
            TokenType.EqualEquals => new LooseEqualityExpression(lhs, rhs),
            TokenType.StrictEqualsEquals => new StrictEqualityExpression(lhs, rhs),
            TokenType.NotEquals => new LooseInequalityExpression(lhs, rhs),
            TokenType.StrictNotEquals => new StrictInequalityExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an equality expression with a token of type {equalityToken.type}"),
        };
    }

    // 13.10 Relational Operators, https://tc39.es/ecma262/#prod-RelationalExpression
    private bool TryParseRelationalExpression(out IExpression? parsedExpression)
    {
        // FIXME: The RelationalOperator in has special logic for parsing in Note 2, handle this rule
        // FIXME: PrivateIdentifiers are parsed here using the in keyword, have a case to handle this rule
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseBitwiseShiftExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have a relational operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsRelationalOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var relationalToken = _consumer.Consume();

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = CreateRelationalOperator(lhs!, rhs!, relationalToken);
        return true;
    }

    private bool IsRelationalOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.LessThan or TokenType.GreaterThan or TokenType.LessThanEqual or 
            TokenType.GreaterThanEqual or TokenType.InstanceOf or TokenType.In => true,
            _ => false
        };
    }

    private IExpression CreateRelationalOperator(IExpression lhs, IExpression rhs, Token relationalToken)
    {
        return relationalToken.type switch
        {
            TokenType.LessThan => new LessThanExpression(lhs, rhs),
            TokenType.GreaterThan => new GreaterThanExpression(lhs, rhs),
            TokenType.LessThanEqual => new LessThanEqualsExpression(lhs, rhs),
            TokenType.GreaterThanEqual => new GreaterThanEqualsExpression(lhs, rhs),
            TokenType.InstanceOf => new InstanceOfExpression(lhs, rhs),
            TokenType.In => new InExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an relational expression with a token of type {relationalToken.type}"),
        };
    }

    // 13.9 Bitwise Shift Operators, https://tc39.es/ecma262/#sec-bitwise-shift-operators 
    private bool TryParseBitwiseShiftExpression(out IExpression? parsedExpression)
    {
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseAdditiveExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have a bitwise shift operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsBitwiseShiftOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var shiftToken = _consumer.Consume();

        // FIXME: Throw a SyntaxError instead
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = CreateBitwiseShiftOperator(lhs!, rhs!, shiftToken);
        return true;
    }

    private bool IsBitwiseShiftOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.LeftShift or TokenType.RightShift or TokenType.UnsignedRightShift => true,
            _ => false,
        };
    }

    private IExpression CreateBitwiseShiftOperator(IExpression lhs, IExpression rhs, Token shiftToken)
    {
        return shiftToken.type switch
        {
            TokenType.LeftShift => new LeftShiftExpression(lhs, rhs),
            TokenType.RightShift => new RightShiftExpression(lhs, rhs),
            TokenType.UnsignedRightShift => new UnsignedRightShiftExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an shift expression with a token of type {shiftToken.type}"),
        };
    }

    // 13.8 Additive Operators, https://tc39.es/ecma262/#sec-additive-operators 
    private bool TryParseAdditiveExpression(out IExpression? parsedExpression)
    {
        if (!TryParsePrimaryExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // If we don't have a additive operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsAdditiveOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var additiveToken = _consumer.Consume();

        // FIXME: Throw a SyntaxError instead
        // FIXME: This doesn't recursively decend and parse "nested" logical expressions correctly
        if (!TryParseExpression(out IExpression? rhs)) throw new InvalidOperationException();

        parsedExpression = CreateAdditiveOperator(lhs!, rhs!, additiveToken);
        return true;
    }

    private bool IsAdditiveOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.Plus or TokenType.Minus => true,
            _ => false,
        };
    }

    private IExpression CreateAdditiveOperator(IExpression lhs, IExpression rhs, Token additiveToken)
    {
        return additiveToken.type switch
        {
            TokenType.Plus => new AdditionExpression(lhs, rhs),
            TokenType.Minus => new SubtractionExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an additive expression with a token of type {additiveToken.type}"),
        };
    }

    // 13.2 Primary Expression, https://tc39.es/ecma262/#sec-primary-expression
    private bool TryParsePrimaryExpression(out IExpression? parsedExpression)
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

        var blockNodes = ParseStatementListWhile(() => _consumer.CanConsume() && !_consumer.IsTokenOfType(TokenType.ClosedBrace));

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

    // 14.7.2 The do-while Statement, https://tc39.es/ecma262/#sec-do-while-statement
    private bool IsDoWhileStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Do);
    }

    private DoWhileStatement ParseDoWhileStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Do);

        var iterationStatement = ParseStatement();

        IExpression? whileExpression;
        if (!TryParseWhileExpression(out whileExpression))
        {
            throw new InvalidOperationException();
        }

        return new DoWhileStatement(whileExpression!, iterationStatement);
    }

    // 14.7.3 The while Statement, https://tc39.es/ecma262/#sec-while-statement
    private bool IsWhileStatement()
    {
        return _consumer.IsTokenOfType(TokenType.While);
    }

    private WhileStatement ParseWhileStatement()
    {
        IExpression? whileExpression;
        if (!TryParseWhileExpression(out whileExpression))
        {
            throw new InvalidOperationException();
        }

        var iterationStatement = ParseStatement();

        return new WhileStatement(whileExpression!, iterationStatement);
    }

    private bool TryParseWhileExpression(out IExpression? whileExpression)
    {
        _consumer.ConsumeTokenOfType(TokenType.While);
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        if (!TryParseExpression(out whileExpression)) return false;

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        return true;
    }

    // 14.7.4 The for Statement, https://tc39.es/ecma262/#sec-for-statement
    private bool IsForStatement()
    {
        return _consumer.IsTokenOfType(TokenType.For);
    }

    private ForStatement ParseForStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.For);
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        TryParseForInitializationExpression(out INode? initializationExpression);

        _consumer.ConsumeTokenOfType(TokenType.SemiColon);

        TryParseExpression(out IExpression? testExpression);

        _consumer.ConsumeTokenOfType(TokenType.SemiColon);

        TryParseExpression(out IExpression? incrementExpression);

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        var iterationStatement = ParseStatement();

        return new ForStatement(initializationExpression, testExpression, incrementExpression, iterationStatement);
    }

    private bool TryParseForInitializationExpression(out INode? initializationExpression)
    {
        // FIXME: This seems a bit clunky
        if (TryParseExpression(out IExpression? expression))
        {
            initializationExpression = expression;
            return true;
        }
        if (IsVarStatement())
        {
            initializationExpression = ParseVarStatement();
            return true;
        }
        if (IsLetDeclaration())
        {
            initializationExpression = ParseLetDeclaration();
            return true;
        }
        if (IsConstDeclaration())
        {
            initializationExpression = ParseConstDeclaration();
            return true;
        }

        initializationExpression = null;
        return false;
    }

    // 14.8 The continue Statement, https://tc39.es/ecma262/#sec-continue-statement
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

    // Tests for 14.9 The break Statement, https://tc39.es/ecma262/#sec-break-statement
    private bool IsBreakStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Break);
    }

    private BreakStatement ParseBreakStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Break);

        Identifier? label = null;
        if (IsIdentifier())
        {
            label = ParseIdentifier();
        }

        return new BreakStatement(label);
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

    // 14.16 The debugger Statement, https://tc39.es/ecma262/#sec-debugger-statement
    private bool IsDebuggerStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Debugger);
    }

    private DebuggerStatement ParseDebuggerStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Debugger);
        return new DebuggerStatement();
    }

    // 15.1 Parameter Lists, https://tc39.es/ecma262/#sec-parameter-lists
    private List<Identifier> ParseFormalParameters()
    {
        // FIXME: Parse FormalParameters as BindingElements, https://tc39.es/ecma262/#prod-FormalParameter
        // FIXME: Parse "Rest" parameters (using the ... operator)
        List<Identifier> formalParameters = new();

        while (IsIdentifier())
        {
            formalParameters.Add(ParseIdentifier());

            if (!_consumer.IsTokenOfType(TokenType.Comma))
            {
                break;
            }

            // NOTE: Trailing commas are allowed in formal parameters
            _consumer.ConsumeTokenOfType(TokenType.Comma);
        }

        return formalParameters;
    }

    // 15.2 Function Definitions, https://tc39.es/ecma262/#sec-function-definitions
    private bool IsFunctionDeclaration()
    {
        return _consumer.IsTokenOfType(TokenType.Function);
    }

    private FunctionDeclaration ParseFunctionDeclaration()
    {
        _consumer.ConsumeTokenOfType(TokenType.Function);

        var identifier = ParseIdentifier();

        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var parameters = ParseFormalParameters();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        var body = ParseFunctionBody();

        // FIXME: Throw SyntaxError if no closed brace
        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new FunctionDeclaration(identifier.Name, parameters, body);
    }

    private List<INode> ParseFunctionBody()
    {
        return ParseStatementListWhile(() => _consumer.CanConsume() && !_consumer.IsTokenOfType(TokenType.ClosedBrace));
    }

    // 15.7 Class Definitions, https://tc39.es/ecma262/#sec-class-definitions
    private bool IsClassDeclaration()
    {
        return _consumer.IsTokenOfType(TokenType.Class);
    }

    private ClassDeclaration ParseClassDeclaration()
    {
        _consumer.ConsumeTokenOfType(TokenType.Class);

        var identifier = ParseIdentifier();

        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        List<MethodDeclaration> methods = new();
        List<MethodDeclaration> staticMethods = new();
        List<FieldDeclaration> fields = new();
        List<FieldDeclaration> staticFields = new();
        ParseClassBody(methods, staticMethods, fields, staticFields);

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new ClassDeclaration(identifier.Name, methods, staticMethods, fields, staticFields);
    }

    private void ParseClassBody(List<MethodDeclaration> methods, List<MethodDeclaration> staticMethods, List<FieldDeclaration> fields, List<FieldDeclaration> staticFields)
    {
        while (!_consumer.IsTokenOfType(TokenType.ClosedBrace))
        {
            ParseClassMember(methods, staticMethods, fields, staticFields);
        }
    }

    private void ParseClassMember(List<MethodDeclaration> methods, List<MethodDeclaration> staticMethods, List<FieldDeclaration> fields, List<FieldDeclaration> staticFields)
    {
        var isStatic = IsStaticClassMember();
        var isPrivate = IsPrivateClassMember();
        var memberIdentifier = ParseClassMemberIdentifier();

        if (IsClassMethod())
        {
            // FIXME: This is a bit janky
            var method = ParseClassMethod(isPrivate, memberIdentifier);
            if (isStatic)
            {
                staticMethods.Add(method);
            }
            else
            {
                methods.Add(method);
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private bool IsStaticClassMember()
    {
        if (!_consumer.IsTokenOfType(TokenType.Identifier)) return false;

        var identifier = _consumer.Peek();
        if (identifier.data != "static") return false;

        _consumer.Consume();
        return true;
    }

    private bool IsPrivateClassMember()
    {
        return _consumer.IsTokenOfType(TokenType.PrivateIdentifier);
    }

    private string ParseClassMemberIdentifier()
    {
        if (_consumer.IsTokenOfType(TokenType.Identifier))
        {
            var identifier = _consumer.Consume();
            return identifier.data;
        }
        else if (_consumer.IsTokenOfType(TokenType.PrivateIdentifier))
        {
            var identifier = _consumer.Consume();
            return identifier.data[1..];
        }

        // FIXME: Throw SyntaxError
        throw new InvalidOperationException();
    }

    private bool IsClassMethod()
    {
        return _consumer.IsTokenOfType(TokenType.OpenParen);
    }

    private MethodDeclaration ParseClassMethod(bool isPrivate, string memberIdentifier)
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var parameters = ParseFormalParameters();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        var body = ParseFunctionBody();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new MethodDeclaration(memberIdentifier, parameters, body, isPrivate);
    }
}
