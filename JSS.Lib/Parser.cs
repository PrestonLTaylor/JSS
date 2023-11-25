using JSS.Lib.AST;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;

namespace JSS.Lib;

internal sealed class Parser
{
    private readonly TokenConsumer _consumer;

    public Parser(string toParse)
    {
        var lexer = new Lexer(toParse);
        _consumer = new TokenConsumer(lexer.Lex().ToList());
    }

    public Script Parse()
    {
        return new Script(ParseScript());
    }

    // 16.1 Scripts, https://tc39.es/ecma262/#sec-scripts
    private StatementList ParseScript()
    {
        return ParseStatementListWhile(() => _consumer.CanConsume());
    }

    // StatementList, https://tc39.es/ecma262/#prod-StatementList
    private StatementList ParseStatementListWhile(Func<bool> shouldParse)
    {
        List<INode> nodes = new();

        while (shouldParse())
        {
            nodes.Add(ParseStatementListItem());
        }

        return new StatementList(nodes);
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

        return ThrowUnexpectedTokenSyntaxError<INode>()!;
    }

    // Statement, https://tc39.es/ecma262/#prod-Statement
    private INode ParseStatement()
    {
        if (TryParseStatement(out INode? statement))
        {
            return statement!;
        }

        return ThrowUnexpectedTokenSyntaxError<INode>()!;
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
        if (TryParseExpressionStatement(out statement))
        {
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
        if (IsSwitchStatement())
        {
            statement = ParseSwitchStatement();
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

    private IExpression ParseExpression()
    {
        if (!TryParseExpression(out IExpression? expression))
        {
            return ThrowUnexpectedTokenSyntaxError<IExpression>()!;
        }

        return expression!;
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
        if (!TryParseLogicalAndExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have an ||, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsLogicalOrOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.Or);

        if (!TryParseLogicalOrExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>(); 

        parsedExpression = new LogicalOrExpression(lhs!, rhs!);
        return true;
    }

    private bool IsLogicalOrOperator()
    {
        return _consumer.IsTokenOfType(TokenType.Or);
    }

    // LogicalANDExpression, https://tc39.es/ecma262/#prod-LogicalANDExpression
    private bool TryParseLogicalAndExpression(out IExpression? parsedExpression)
    {
        if (!TryParseBitwiseORExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have an &&, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsLogicalAndOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.And);

        if (!TryParseLogicalAndExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = new LogicalAndExpression(lhs!, rhs!);
        return true;
    }

    private bool IsLogicalAndOperator()
    {
        return _consumer.IsTokenOfType(TokenType.And);
    }

    // BitwiseORExpression, https://tc39.es/ecma262/#prod-BitwiseORExpression
    private bool TryParseBitwiseORExpression(out IExpression? parsedExpression)
    {
        if (!TryParseBitwiseXORExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have an |, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsBitwiseOrOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseOr);

        if (!TryParseBitwiseORExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = new BitwiseOrExpression(lhs!, rhs!);
        return true;
    }

    private bool IsBitwiseOrOperator()
    {
        return _consumer.IsTokenOfType(TokenType.BitwiseOr);
    }

    // BitwiseXORExpression, https://tc39.es/ecma262/#prod-BitwiseXORExpression
    private bool TryParseBitwiseXORExpression(out IExpression? parsedExpression)
    {
        if (!TryParseBitwiseAndExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have an ^, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsBitwiseXorOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseXor);

        if (!TryParseBitwiseXORExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = new BitwiseXorExpression(lhs!, rhs!);
        return true;
    }

    private bool IsBitwiseXorOperator()
    {
        return _consumer.IsTokenOfType(TokenType.BitwiseXor);
    }

    // BitwiseANDExpression, https://tc39.es/ecma262/#prod-BitwiseANDExpression
    private bool TryParseBitwiseAndExpression(out IExpression? parsedExpression)
    {
        if (!TryParseEqualityExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have an &, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsBitwiseAndOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.BitwiseAnd);

        if (!TryParseBitwiseAndExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = new BitwiseAndExpression(lhs!, rhs!);
        return true;
    }

    private bool IsBitwiseAndOperator()
    {
        return _consumer.IsTokenOfType(TokenType.BitwiseAnd);
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

        // NOTE: If we don't have an equality operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsEqualityOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var equalityToken = _consumer.Consume();

        if (!TryParseEqualityExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

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
        if (!TryParseBitwiseShiftExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have a relational operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsRelationalOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var relationalToken = _consumer.Consume();

        if (!TryParseRelationalExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

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
        if (!TryParseAdditiveExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have a bitwise shift operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsBitwiseShiftOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var shiftToken = _consumer.Consume();

        if (!TryParseBitwiseShiftExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

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
        if (!TryParseMultiplicativeExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have a additive operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsAdditiveOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var additiveToken = _consumer.Consume();

        if (!TryParseAdditiveExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

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

    // 13.7 Multiplicative Operators, https://tc39.es/ecma262/#sec-multiplicative-operators 
    private bool TryParseMultiplicativeExpression(out IExpression? parsedExpression)
    {
        if (!TryParseExponentiationExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have a multiplicative operator, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsMultiplicativeOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        var multiplicativeToken = _consumer.Consume();

        if (!TryParseMultiplicativeExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = CreateMultiplicativeOperator(lhs!, rhs!, multiplicativeToken);
        return true;
    }

    private bool IsMultiplicativeOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.Multiply or TokenType.Division or TokenType.Modulo => true,
            _ => false,
        };
    }

    private IExpression CreateMultiplicativeOperator(IExpression lhs, IExpression rhs, Token multiplicativeToken)
    {
        return multiplicativeToken.type switch
        {
            TokenType.Multiply => new MultiplicationExpression(lhs, rhs),
            TokenType.Division => new DivisionExpression(lhs, rhs),
            TokenType.Modulo => new ModuloExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an multiplicative expression with a token of type {multiplicativeToken.type}"),
        };
    }

    // 13.6 Exponentiation Operator, https://tc39.es/ecma262/#sec-exp-operator
    private bool TryParseExponentiationExpression(out IExpression? parsedExpression)
    {
        // FIXME: This is a bit janky
        if (IsUnaryOperator() && TryParseUnaryExpression(out parsedExpression))
        {
            if (IsExponentiationOperator()) ErrorHelper.ThrowSyntaxError(ErrorType.UnaryLHSOfExponentiation);
            return true;
        }
        if (!TryParseUpdateExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // NOTE: If we don't have a **, that means we've reached the end of the expression and lhs is the fully parsed expression
        if (!IsExponentiationOperator())
        {
            parsedExpression = lhs;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.Exponentiation);

        if (!TryParseExponentiationExpression(out IExpression? rhs)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = new ExponentiationExpression(lhs!, rhs!);
        return true;
    }

    private bool IsExponentiationOperator()
    {
        return _consumer.IsTokenOfType(TokenType.Exponentiation);
    }

    // 13.5 Unary Operators, https://tc39.es/ecma262/#prod-UnaryExpression
    private bool TryParseUnaryExpression(out IExpression? parsedExpression)
    {
        // FIXME: Handle parsing of the await keyword acording to the spec
        if (!IsUnaryOperator())
        {
            return TryParseUpdateExpression(out parsedExpression);
        }

        var unaryToken = _consumer.Consume();
        if (!TryParseUnaryExpression(out IExpression? innerExpression)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = CreateUnaryExpression(innerExpression!, unaryToken);
        return true;
    }

    private bool IsUnaryOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.Delete or TokenType.Void or TokenType.TypeOf or TokenType.Plus
            or TokenType.Minus or TokenType.BitwiseNot or TokenType.Not => true,
            _ => false
        };
    }

    private IExpression CreateUnaryExpression(IExpression innerExpression, Token unaryToken)
    {
        return unaryToken.type switch
        {
            TokenType.Delete => new DeleteExpression(innerExpression),
            TokenType.Void => new VoidExpression(innerExpression),
            TokenType.TypeOf => new TypeOfExpression(innerExpression),
            TokenType.Plus => new UnaryPlusExpression(innerExpression),
            TokenType.Minus => new UnaryMinusExpression(innerExpression),
            TokenType.BitwiseNot => new BitwiseNotExpression(innerExpression),
            TokenType.Not => new LogicalNotExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an unary expression with a token of type {unaryToken.type}"),
        };
    }

    // 13.4 Update Expressions, https://tc39.es/ecma262/#sec-update-expressions
    private bool TryParseUpdateExpression(out IExpression? parsedExpression)
    {
        // FIXME: Early Errors for parsing update expressions, https://tc39.es/ecma262/#sec-update-expressions-static-semantics-early-errors
        if (TryParsePrefixUpdateExpression(out parsedExpression))
        {
            return true;
        }
        if (!TryParseLeftHandSideExpression(out IExpression? innerExpression))
        {
            return false;
        }

        // NOTE: If there is no postfix update operator, then the inner expression is the fully parsed expression
        if (!IsUpdateOperator())
        {
            parsedExpression = innerExpression;
            return true;
        }

        var postfixUpdateToken = _consumer.Consume();
        parsedExpression = CreatePostfixUpdateExpression(innerExpression!, postfixUpdateToken);
        return true;
    }

    private bool TryParsePrefixUpdateExpression(out IExpression? parsedExpression)
    {
        if (!IsUpdateOperator())
        {
            parsedExpression = null;
            return false;
        }

        var prefixUpdateToken = _consumer.Consume();
        if (!TryParseUnaryExpression(out IExpression? innerExpression)) ThrowUnexpectedTokenSyntaxError<bool>();

        parsedExpression = CreatePrefixUpdateExpression(innerExpression!, prefixUpdateToken);
        return true;
    }

    private bool IsUpdateOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.Increment or TokenType.Decrement => true,
            _ => false,
        };
    }

    private IExpression CreatePrefixUpdateExpression(IExpression innerExpression, Token prefixUpdateToken)
    {
        return prefixUpdateToken.type switch
        {
            TokenType.Increment => new PrefixIncrementExpression(innerExpression),
            TokenType.Decrement => new PrefixDecrementExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create a prefix update expression with a token of type {prefixUpdateToken.type}"),
        };
    }

    private IExpression CreatePostfixUpdateExpression(IExpression innerExpression, Token prefixUpdateToken)
    {
        return prefixUpdateToken.type switch
        {
            TokenType.Increment => new PostfixIncrementExpression(innerExpression),
            TokenType.Decrement => new PostfixDecrementExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create a prefix update expression with a token of type {prefixUpdateToken.type}"),
        };
    }

    // 13.3 Left-Hand-Side Expressions, https://tc39.es/ecma262/#sec-left-hand-side-expressions
    private bool TryParseLeftHandSideExpression(out IExpression? parsedExpression)
    {
        // FIXME: Implement parsing of CallExpressions
        // FIXME: Implement parsing of OptionalExpressions
        // FIXME: Early Errors for parsing according to the spec
        if (TryParseNewExpression(out parsedExpression))
        {
            return true;
        }
        // NOTE: Technically this rule is inside new expression, but for simplicity we parse it seperately here
        if (TryParseMemberExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // NewExpression, https://tc39.es/ecma262/#prod-NewExpression
    private bool TryParseNewExpression(out IExpression? parsedExpression)
    {
        if (!IsNewExpression())
        {
            parsedExpression = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.New);

        // NOTE: This happens when there is a new without a member expression at the end of the new chain
        if (!TryParseInnerNewExpression(out IExpression? innerNewExpression)) ThrowUnexpectedTokenSyntaxError<bool>();

        // NOTE: This rule is duplicated in MemberExpression, however it is simpilier to have it
        List<IExpression> newArguments = new();
        TryParseArguments(newArguments);

        parsedExpression = new NewExpression(innerNewExpression!, newArguments);
        return true;
    }

    private bool IsNewExpression()
    {
        return _consumer.IsTokenOfType(TokenType.New);
    }

    private bool TryParseInnerNewExpression(out IExpression? parsedExpression)
    {
        if (TryParseNewExpression(out parsedExpression))
        {
            return true;
        }
        if (TryParseMemberExpression(out parsedExpression))
        {
            return true;
        }

        return false;
    }

    // Arguments, https://tc39.es/ecma262/#prod-Arguments
    private bool IsArguments()
    {
        return _consumer.IsTokenOfType(TokenType.OpenParen);
    }

    private bool TryParseArguments(List<IExpression> arguments)
    {
        if (!IsArguments())
        {
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        // FIXME: Implement parsing of the spread (...) operator
        while (TryParseAssignmentExpression(out IExpression? argument))
        {
            arguments.Add(argument!);

            if (!_consumer.IsTokenOfType(TokenType.Comma))
            {
                break;
            }

            // NOTE: Trailing commas are allowed in formal parameters
            _consumer.ConsumeTokenOfType(TokenType.Comma);
        }

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        return true;
    }

    private List<IExpression> ParseArguments()
    {
        List<IExpression> arguments = new();
        if (TryParseArguments(arguments))
        {
            return arguments;
        }

        // FIXME: Fixs this janky way of throwing errors
        return ThrowUnexpectedTokenSyntaxError<List<IExpression>>()!;
    }

    // MemberExpression, https://tc39.es/ecma262/#prod-MemberExpression
    private bool TryParseMemberExpression(out IExpression? parsedExpression)
    {
        // FIXME: Parse the rest of rules for MemberExpressions
        if (!TryParseInnerMemberExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        parsedExpression = ParseOuterMemberExpression(lhs!);
        return true;
    }

    private IExpression ParseMemberExpression()
    {
        if (TryParseMemberExpression(out IExpression? parsedExpression))
        {
            return parsedExpression!;
        }

        return ThrowUnexpectedTokenSyntaxError<IExpression>()!;
    }

    private bool TryParseInnerMemberExpression(out IExpression? parsedExpression)
    {
        // NOTE: This is the base case for the recursive member expression definition
        // FIXME: Parse the rest of the inner rules for MemberExpressions
        if (TryParsePrimaryExpression(out parsedExpression))
        {
            return true;
        }
        if (TryParseSuperExpression(out parsedExpression))
        {
            return true;
        }
        // NOTE: This is repeated in NewExpression, however this makes parsing simplier to do
        if (TryParseNewWithArgumentsExpression(out parsedExpression))
        {
            return true;
        }

        parsedExpression = null;
        return false;
    }

    private bool TryParseNewWithArgumentsExpression(out IExpression? parsedExpression)
    {
        if (!IsNewExpression())
        {
            parsedExpression = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.New);

        var innerExpression = ParseMemberExpression();

        List<IExpression> newArguments = ParseArguments();

        parsedExpression = new NewExpression(innerExpression, newArguments);
        return true;
    }

    private IExpression ParseOuterMemberExpression(IExpression lhs)
    {
        // FIXME: Parse TemplateLiterals property expressions
        if (IsComputedProperty())
        {
            return ParseComputedPropertyExpression(lhs);
        }
        if (IsProperty())
        {
            return ParsePropertyExpression(lhs);
        }
        // FIXME: Parse SuperCalls and ImportCalls
        if (IsCall())
        {
            return ParseCallExpression(lhs);
        }

        return lhs;
    }

    private bool IsComputedProperty()
    {
        return _consumer.IsTokenOfType(TokenType.OpenSquare);
    }

    private IExpression ParseComputedPropertyExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenSquare);

        var rhs = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedSquare);

        var newLhs = new ComputedPropertyExpression(lhs, rhs);

        return ParseOuterMemberExpression(newLhs);
    }

    private bool IsProperty()
    {
        return _consumer.IsTokenOfType(TokenType.Dot);
    }

    private IExpression ParsePropertyExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.Dot);

        // FIXME: Handle private identifiers
        // FIXME: This should be any valid identifier string including reserved words etc.
        var rhs = ParseIdentifier();

        var newLhs = new PropertyExpression(lhs, rhs.Name);

        return ParseOuterMemberExpression(newLhs);
    }

    private bool IsCall()
    {
        return IsArguments();
    }

    private IExpression ParseCallExpression(IExpression lhs)
    {
        var arguments = ParseArguments();
        var newLhs = new CallExpression(lhs, arguments);
        return ParseOuterMemberExpression(newLhs);
    }

    // SuperProperty, https://tc39.es/ecma262/#prod-SuperProperty
    private bool TryParseSuperExpression(out IExpression? parsedExpression)
    {
        if (!IsSuperExpression())
        {
            parsedExpression = null;
            return false;
        }

        _consumer.ConsumeTokenOfType(TokenType.Super);

        if (IsComputedProperty())
        {
            parsedExpression = ParseSuperComputedPropertyExpression();
            return true;
        }
        if (IsProperty())
        {
            parsedExpression = ParseSuperPropertyExpression();
            return true;
        }

        parsedExpression = null;
        return false;
    }

    private bool IsSuperExpression()
    {
        return _consumer.IsTokenOfType(TokenType.Super);
    }

    private SuperComputedPropertyExpression ParseSuperComputedPropertyExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenSquare);

        var computedProperty = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedSquare);

        return new SuperComputedPropertyExpression(computedProperty);
    }

    private SuperPropertyExpression ParseSuperPropertyExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.Dot);

        // FIXME: This should be any valid identifier string including reserved words etc.
        var identifier = ParseIdentifier();

        return new SuperPropertyExpression(identifier.Name);
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
        if (IsParenthesizedExpression())
        {
            parsedExpression = ParseParenthesizedExpression();
            return true;
        }

        parsedExpression = null;
        return false;
    }

    // ParenthesizedExpression, https://tc39.es/ecma262/#prod-ParenthesizedExpression
    private bool IsParenthesizedExpression()
    {
        return _consumer.IsTokenOfType(TokenType.OpenParen);
    }

    private IExpression ParseParenthesizedExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var parsedExpression = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        // FIXME: This expression might have to be wrapped if needed in the AST according to the spec
        return parsedExpression!;
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

        var statementList = ParseStatementListWhile(() => _consumer.CanConsume() && !_consumer.IsTokenOfType(TokenType.ClosedBrace));

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new Block(statementList);
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
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single let declaration
        INode? initializer = null;
        if (IsInitializer())
        {
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
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single const declaration
        if (!_consumer.IsTokenOfType(TokenType.Assignment)) ErrorHelper.ThrowSyntaxError(ErrorType.ConstWithoutInitializer);

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
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single var statement 
        INode? initializer = null;
        if (IsInitializer())
        {
            initializer = ParseInitializer();
        }

        return new VarStatement(identifierToken.data, initializer);
    }

    private bool IsInitializer()
    {
        return _consumer.IsTokenOfType(TokenType.Assignment);
    }

    private INode ParseInitializer()
    {
        _consumer.ConsumeTokenOfType(TokenType.Assignment);

        if (TryParseAssignmentExpression(out IExpression? parsedExpression))
        {
            return parsedExpression!;
        }

        return ThrowUnexpectedTokenSyntaxError<INode>()!;
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

    // 14.5 Expression Statement, https://tc39.es/ecma262/#prod-ExpressionStatement
    private bool TryParseExpressionStatement(out INode? parsedExpressionStatement)
    {
        if (IsAmbigiousForExpressionStatement())
        {
            parsedExpressionStatement = null;
            return false;
        }

        if (!TryParseExpression(out IExpression? expression))
        {
            parsedExpressionStatement = null;
            return false;
        }

        parsedExpressionStatement = new ExpressionStatement(expression!);
        return true;
    }

    private bool IsAmbigiousForExpressionStatement()
    {
        return _consumer.CanConsume() && _consumer.Peek().type switch
        {
            TokenType.OpenBrace or TokenType.Function or TokenType.Class => true,
            // FIXME: Check for async function
            // FIXME: This is a bit janky
            TokenType.Let => _consumer.CanConsume(1) && _consumer.Peek(1).type == TokenType.OpenSquare,
            _ => false,
        };
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

        var ifExpression = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        var ifCaseStatement = ParseStatement();
        TryParseElseStatement(out INode? elseCaseStatement);

        return new IfStatement(ifExpression, ifCaseStatement, elseCaseStatement);
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
        if (!TryParseWhileExpression(out whileExpression)) ThrowUnexpectedTokenSyntaxError<DoWhileStatement>();

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
        if (!TryParseWhileExpression(out whileExpression)) ThrowUnexpectedTokenSyntaxError<WhileStatement>();

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

    // 14.12 The switch Statement, https://tc39.es/ecma262/#sec-switch-statement
    public bool IsSwitchStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Switch);
    }

    private SwitchStatement ParseSwitchStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Switch); 
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var switchExpression = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);
        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        List<CaseBlock> caseBlocks = new();
        ParseCaseBlocks(caseBlocks);

        var defaultBlock = ParseDefaultCase();

        ParseCaseBlocks(caseBlocks);

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new SwitchStatement(switchExpression, caseBlocks, defaultBlock);
    }

    private void ParseCaseBlocks(List<CaseBlock> caseBlocks)
    {
        while (IsCaseBlock())
        {
            caseBlocks.Add(ParseCaseBlock());
        }
    }

    private bool IsCaseBlock()
    {
        return _consumer.IsTokenOfType(TokenType.Case);
    }

    private CaseBlock ParseCaseBlock()
    {
        _consumer.ConsumeTokenOfType(TokenType.Case);

        var caseExpression = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.Colon);

        var caseStatements = ParseSwitchCaseStatementList();

        return new CaseBlock(caseExpression, caseStatements);
    }

    private DefaultBlock? ParseDefaultCase()
    {
        if (!IsDefaultCase()) return null;

        _consumer.ConsumeTokenOfType(TokenType.Default);
        _consumer.ConsumeTokenOfType(TokenType.Colon);

        var caseStatements = ParseSwitchCaseStatementList();

        return new DefaultBlock(caseStatements);
    }
    
    private bool IsDefaultCase()
    {
        return _consumer.IsTokenOfType(TokenType.Default);
    }

    private StatementList ParseSwitchCaseStatementList()
    {
        return ParseStatementListWhile(() =>
        {
            return _consumer.Peek().type switch
            {
                TokenType.Case or TokenType.Default or TokenType.ClosedBrace => false,
                _ => true,
            };
        });
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
        var throwExpression = ParseExpression();

        return new ThrowStatement(throwExpression);
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

        if (!didParseCatch && !didParseFinally) ErrorHelper.ThrowSyntaxError(ErrorType.TryWithoutCatchOrFinally);

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

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new FunctionDeclaration(identifier.Name, parameters, body);
    }

    private StatementList ParseFunctionBody()
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

        return ThrowUnexpectedTokenSyntaxError<string>()!;
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

    // NOTE: We need to return a null for functions that return a value
    public T? ThrowUnexpectedTokenSyntaxError<T>()
    {
        if (_consumer.CanConsume())
        {
            var unexpectedToken = _consumer.Peek();
            ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedToken, unexpectedToken.data);
        }

        ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedEOF);
        return default;
    }
}
