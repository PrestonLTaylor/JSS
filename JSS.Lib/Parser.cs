using JSS.Lib.AST;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;

namespace JSS.Lib;

public sealed class Parser
{
    private readonly TokenConsumer _consumer;

    public Parser(string toParse)
    {
        var lexer = new Lexer(toParse);
        _consumer = new TokenConsumer(lexer.Lex().ToList());
    }

    // NOTE: sourceText is provided in the constructor, the VM holds the realm and we don't use hostDefined right now
    // 16.1.5 ParseScript ( sourceText, realm, hostDefined ), https://tc39.es/ecma262/#sec-parse-script
    public Script Parse(VM vm)
    {
        // 1. Let script be ParseText(sourceText, Script).
        var isStrict = ParseDirectivePrologue();
        var script = ParseScript();

        // NOTE: This step is handled as C# exceptions
        // 2. If script is a List of errors, return script.

        // 3. Return Script Record { [[Realm]]: realm, [[ECMAScriptCode]]: script, [[LoadedModules]]: « », [[HostDefined]]: hostDefined }.
        return new Script(vm, script, isStrict);
    }

    // 11.2.1 Directive Prologues and the Use Strict Directive, https://tc39.es/ecma262/#directive-prologue
    private bool ParseDirectivePrologue()
    {
        // NOTE/FIXME: We parse expression statements at the beginning of a script/function twice, if this becomes a perf issue, then we should not do parsing twice.
        // A Directive Prologue is the longest sequence of ExpressionStatements occurring as the initial StatementListItems or ModuleItems of a FunctionBody,
        // a ScriptBody, or a ModuleBody and where each ExpressionStatement in the sequence consists entirely of a StringLiteral token FIXME: followed by a semicolon.
        var isStrict = false;
        var beforeParseIndex = _consumer.Index;

        // NOTE: We need to check if the current element is a string literal, as otherwise, we'll infinitely recurse back to this function
        // if the current element is a function declaration/expression.
        while (IsStringLiteral() && TryParseExpressionStatement(out var expression))
        {
            var expressionStatement = expression as ExpressionStatement;
            if (expressionStatement!.Expression is not StringLiteral) break;
            var stringLiteral = expressionStatement.Expression as StringLiteral;
            if (stringLiteral!.Value == "use strict") isStrict = true;
        }

        _consumer.Rewind(beforeParseIndex);
        return isStrict;
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

        while (true)
        {
            _consumer.IgnoreLineTerminators();
            if (!shouldParse()) break;
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
        if (IsLabelledStatement())
        {
            statement = ParseLabelledStatement();
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
    private IExpression ParseAssignmentExpression()
    {
        if (!TryParseAssignmentExpression(out var assignmentExpression))
        {
            return ThrowUnexpectedTokenSyntaxError<IExpression>()!;
        }

        return assignmentExpression!;
    }

    private bool TryParseAssignmentExpression(out IExpression? parsedExpression)
    {
        if (!TryParseConditionalExpression(out IExpression? lhs))
        {
            parsedExpression = null;
            return false;
        }

        // FIXME: Use this early error rule https://tc39.es/ecma262/#sec-static-semantics-assignmenttargettype
        if (!IsLeftHandSideExpressionNode(lhs!) || !TryParseRhsOfAssignmentExpression(lhs!, out parsedExpression))
        {
            parsedExpression = lhs;
        }

        return true;
    }

    private bool IsLeftHandSideExpressionNode(IExpression expression)
    {
        return expression is Identifier or PropertyExpression or ComputedPropertyExpression or SuperPropertyExpression or SuperComputedPropertyExpression;
    }

    private bool TryParseRhsOfAssignmentExpression(IExpression lhs, out IExpression? parsedExpression)
    {
        if (IsBasicAssignmentOperator())
        {
            parsedExpression = ParseBasicAssignmentExpression(lhs);
            return true;
        }
        if (IsLogicalAndAssignmentOperator())
        {
            parsedExpression = ParseLogicalAndAssignmentExpression(lhs);
            return true;
        }
        if (IsLogicalOrAssignmentOperator())
        {
            parsedExpression = ParseLogicalOrAssignmentExpression(lhs);
            return true;
        }
        if (IsNullCoalescingAssignmentOperator())
        {
            parsedExpression = ParseNullCoalescingAssignmentExpression(lhs);
            return true;
        }
        if (IsBinaryOpAssignmentOperator())
        {
            parsedExpression = ParseBinaryOpAssignmentExpression(lhs);
            return true;
        }

        parsedExpression = null;
        return false;
    }

    private bool IsBasicAssignmentOperator()
    {
        return _consumer.IsTokenOfType(TokenType.Assignment);
    }

    private IExpression ParseBasicAssignmentExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.Assignment);

        if (!TryParseAssignmentExpression(out IExpression? rhs))
        {
            ThrowUnexpectedTokenSyntaxError<IExpression>();
        }

        return new BasicAssignmentExpression(lhs, rhs!);
    }

    private bool IsLogicalAndAssignmentOperator()
    {
        return _consumer.IsTokenOfType(TokenType.AndAssignment);
    }

    private IExpression ParseLogicalAndAssignmentExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.AndAssignment);

        if (!TryParseAssignmentExpression(out IExpression? rhs))
        {
            ThrowUnexpectedTokenSyntaxError<IExpression>();
        }

        return new LogicalAndAssignmentExpression(lhs, rhs!);
    }

    private bool IsLogicalOrAssignmentOperator()
    {
        return _consumer.IsTokenOfType(TokenType.OrAssignment);
    }

    private IExpression ParseLogicalOrAssignmentExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.OrAssignment);

        if (!TryParseAssignmentExpression(out IExpression? rhs))
        {
            ThrowUnexpectedTokenSyntaxError<IExpression>();
        }

        return new LogicalOrAssignmentExpression(lhs, rhs!);
    }

    private bool IsNullCoalescingAssignmentOperator()
    {
        return _consumer.IsTokenOfType(TokenType.NullCoalescingAssignment);
    }

    private IExpression ParseNullCoalescingAssignmentExpression(IExpression lhs)
    {
        _consumer.ConsumeTokenOfType(TokenType.NullCoalescingAssignment);

        if (!TryParseAssignmentExpression(out IExpression? rhs))
        {
            ThrowUnexpectedTokenSyntaxError<IExpression>();
        }

        return new NullCoalescingAssignmentExpression(lhs, rhs!);
    }

    private bool IsBinaryOpAssignmentOperator()
    {
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.ExponentiationAssignment or TokenType.MultiplyAssignment or TokenType.DivisionAssignment 
                or TokenType.ModuloAssignment or TokenType.PlusAssignment or TokenType.MinusAssignment or TokenType.LeftShiftAssignment
                or TokenType.RightShiftAssignment or TokenType.UnsignedRightShiftAssignment or TokenType.BitwiseAndAssignment
                or TokenType.BitwiseXorAssignment or TokenType.BitwiseOrAssignment => true,
            _ => false,
        };
    }

    private IExpression ParseBinaryOpAssignmentExpression(IExpression lhs)
    {
        var binaryOpToken = _consumer.Consume();
        var binaryOpType = BinaryOpTokenToBinaryOpType(binaryOpToken);

        if (!TryParseAssignmentExpression(out IExpression? rhs))
        {
            ThrowUnexpectedTokenSyntaxError<IExpression>();
        }

        return new BinaryOpAssignmentExpression(lhs, binaryOpType, rhs!);
    }

    private BinaryOpType BinaryOpTokenToBinaryOpType(Token binaryOp)
    {
        return binaryOp.Type switch
        {
            TokenType.ExponentiationAssignment => BinaryOpType.Exponentiate, 
            TokenType.MultiplyAssignment => BinaryOpType.Multiply,
            TokenType.DivisionAssignment => BinaryOpType.Divide,
            TokenType.ModuloAssignment => BinaryOpType.Remainder,
            TokenType.PlusAssignment => BinaryOpType.Add,
            TokenType.MinusAssignment => BinaryOpType.Subtract,
            TokenType.LeftShiftAssignment => BinaryOpType.LeftShift,
            TokenType.RightShiftAssignment => BinaryOpType.SignedRightShift,
            TokenType.UnsignedRightShiftAssignment => BinaryOpType.UnsignedRightShift,
            TokenType.BitwiseAndAssignment => BinaryOpType.BitwiseAND,
            TokenType.BitwiseXorAssignment => BinaryOpType.BitwiseXOR,
            TokenType.BitwiseOrAssignment => BinaryOpType.BitwiseOR,
            _ => throw new InvalidOperationException($"Parser Bug: Tried to get a binary op type with a token of type {binaryOp.Type}"),
        };
    }

    // ConditionalExpression, https://tc39.es/ecma262/#prod-ConditionalExpression
    private bool TryParseConditionalExpression(out IExpression? parsedExpression)
    {
        if (!TryParseShortCircuitExpression(out var shortCircuitExpression))
        {
            parsedExpression = null;
            return false;
        }

        if (!IsTernaryOperator())
        {
            parsedExpression = shortCircuitExpression;
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.Ternary);

        var trueCaseExpression = ParseAssignmentExpression();

        _consumer.ConsumeTokenOfType(TokenType.Colon);

        var falseCaseExpression = ParseAssignmentExpression();

        parsedExpression = new TernaryExpression(shortCircuitExpression!, trueCaseExpression, falseCaseExpression);
        return true;
    }

    private bool IsTernaryOperator()
    {
        return _consumer.IsTokenOfType(TokenType.Ternary);
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.EqualEquals or TokenType.StrictEqualsEquals or TokenType.NotEquals or TokenType.StrictNotEquals => true,
            _ => false,
        };
    }

    private IExpression CreateEqualityExpression(IExpression lhs, IExpression rhs, Token equalityToken)
    {
        return equalityToken.Type switch
        {
            TokenType.EqualEquals => new LooseEqualityExpression(lhs, rhs),
            TokenType.StrictEqualsEquals => new StrictEqualityExpression(lhs, rhs),
            TokenType.NotEquals => new LooseInequalityExpression(lhs, rhs),
            TokenType.StrictNotEquals => new StrictInequalityExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an equality expression with a token of type {equalityToken.Type}"),
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.LessThan or TokenType.GreaterThan or TokenType.LessThanEqual or 
            TokenType.GreaterThanEqual or TokenType.InstanceOf or TokenType.In => true,
            _ => false
        };
    }

    private IExpression CreateRelationalOperator(IExpression lhs, IExpression rhs, Token relationalToken)
    {
        return relationalToken.Type switch
        {
            TokenType.LessThan => new LessThanExpression(lhs, rhs),
            TokenType.GreaterThan => new GreaterThanExpression(lhs, rhs),
            TokenType.LessThanEqual => new LessThanEqualsExpression(lhs, rhs),
            TokenType.GreaterThanEqual => new GreaterThanEqualsExpression(lhs, rhs),
            TokenType.InstanceOf => new InstanceOfExpression(lhs, rhs),
            TokenType.In => new InExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an relational expression with a token of type {relationalToken.Type}"),
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.LeftShift or TokenType.RightShift or TokenType.UnsignedRightShift => true,
            _ => false,
        };
    }

    private IExpression CreateBitwiseShiftOperator(IExpression lhs, IExpression rhs, Token shiftToken)
    {
        return shiftToken.Type switch
        {
            TokenType.LeftShift => new LeftShiftExpression(lhs, rhs),
            TokenType.RightShift => new RightShiftExpression(lhs, rhs),
            TokenType.UnsignedRightShift => new UnsignedRightShiftExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an shift expression with a token of type {shiftToken.Type}"),
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.Plus or TokenType.Minus => true,
            _ => false,
        };
    }

    private IExpression CreateAdditiveOperator(IExpression lhs, IExpression rhs, Token additiveToken)
    {
        return additiveToken.Type switch
        {
            TokenType.Plus => new AdditionExpression(lhs, rhs),
            TokenType.Minus => new SubtractionExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an additive expression with a token of type {additiveToken.Type}"),
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.Multiply or TokenType.Division or TokenType.Modulo => true,
            _ => false,
        };
    }

    private IExpression CreateMultiplicativeOperator(IExpression lhs, IExpression rhs, Token multiplicativeToken)
    {
        return multiplicativeToken.Type switch
        {
            TokenType.Multiply => new MultiplicationExpression(lhs, rhs),
            TokenType.Division => new DivisionExpression(lhs, rhs),
            TokenType.Modulo => new ModuloExpression(lhs, rhs),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an multiplicative expression with a token of type {multiplicativeToken.Type}"),
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.Delete or TokenType.Void or TokenType.TypeOf or TokenType.Plus
            or TokenType.Minus or TokenType.BitwiseNot or TokenType.Not => true,
            _ => false
        };
    }

    private IExpression CreateUnaryExpression(IExpression innerExpression, Token unaryToken)
    {
        return unaryToken.Type switch
        {
            TokenType.Delete => new DeleteExpression(innerExpression),
            TokenType.Void => new VoidExpression(innerExpression),
            TokenType.TypeOf => new TypeOfExpression(innerExpression),
            TokenType.Plus => new UnaryPlusExpression(innerExpression),
            TokenType.Minus => new UnaryMinusExpression(innerExpression),
            TokenType.BitwiseNot => new BitwiseNotExpression(innerExpression),
            TokenType.Not => new LogicalNotExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create an unary expression with a token of type {unaryToken.Type}"),
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
        // NOTE: No line terminators are allowed between an expression and postfix update operator
        if (_consumer.CanConsume(-1) && _consumer.IsLineTerminator(-1) || !IsUpdateOperator())
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
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.Increment or TokenType.Decrement => true,
            _ => false,
        };
    }

    private IExpression CreatePrefixUpdateExpression(IExpression innerExpression, Token prefixUpdateToken)
    {
        return prefixUpdateToken.Type switch
        {
            TokenType.Increment => new PrefixIncrementExpression(innerExpression),
            TokenType.Decrement => new PrefixDecrementExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create a prefix update expression with a token of type {prefixUpdateToken.Type}"),
        };
    }

    private IExpression CreatePostfixUpdateExpression(IExpression innerExpression, Token prefixUpdateToken)
    {
        return prefixUpdateToken.Type switch
        {
            TokenType.Increment => new PostfixIncrementExpression(innerExpression),
            TokenType.Decrement => new PostfixDecrementExpression(innerExpression),
            _ => throw new InvalidOperationException($"Parser Bug: Tried to create a prefix update expression with a token of type {prefixUpdateToken.Type}"),
        };
    }

    // 13.3 Left-Hand-Side Expressions, https://tc39.es/ecma262/#sec-left-hand-side-expressions
    private bool TryParseLeftHandSideExpression(out IExpression? parsedExpression)
    {
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

        // NOTE: This is a hack to prevent a call expression from being parsed instead of a new expression
        // with arguments
        if (innerNewExpression is CallExpression)
        {
            parsedExpression = ParseNewExpressionWithCallExpressionRHS((innerNewExpression as CallExpression)!);
            return true;
        }

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

    private NewExpression ParseNewExpressionWithCallExpressionRHS(CallExpression rhs)
    {
        return new NewExpression(rhs.Lhs, rhs.Arguments);
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
        if (IsArrayLiteral())
        {
            parsedExpression = ParseArrayLiteral();
            return true;
        }
        if (IsObjectLiteral())
        {
            parsedExpression = ParseObjectLiteral();
            return true;
        }
        if (IsParenthesizedExpression())
        {
            parsedExpression = ParseParenthesizedExpression();
            return true;
        }
        if (IsFunctionDeclaration())
        {
            parsedExpression = ParseFunctionExpression();
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
        return new Identifier(identifierToken.Data);
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
        var booleanValue = booleanToken.Type == TokenType.True;
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
        var numericValue = double.Parse(numericToken.Data);
        return new NumericLiteral(numericValue);
    }

    private bool IsStringLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.String);
    }

    private StringLiteral ParseStringLiteral()
    {
        var stringLiteral = _consumer.ConsumeTokenOfType(TokenType.String);
        var stringValue = stringLiteral.Data[1..^1];
        return new StringLiteral(stringValue);
    }

    // 13.2.4 Array Initializer, https://tc39.es/ecma262/#sec-array-initializer
    private bool IsArrayLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.OpenSquare);
    }

    private ArrayLiteral ParseArrayLiteral()
    {
        // FIXME: Implement parsing of ElementList
        _consumer.ConsumeTokenOfType(TokenType.OpenSquare);
        _consumer.ConsumeTokenOfType(TokenType.ClosedSquare);

        return new ArrayLiteral();
    }

    // 13.2.5 Object Initializer, https://tc39.es/ecma262/#prod-ObjectLiteral
    private bool IsObjectLiteral()
    {
        return _consumer.IsTokenOfType(TokenType.OpenBrace);
    }

    private ObjectLiteral ParseObjectLiteral()
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        var propertyDefinitionList = ParsePropertyDefinitionList();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new ObjectLiteral(propertyDefinitionList);
    }

    private List<IPropertyDefinition> ParsePropertyDefinitionList()
    {
        List<IPropertyDefinition> propertyDefinitionList = new();

        while (TryParsePropertyName(out var propertyName))
        {
            _consumer.ConsumeTokenOfType(TokenType.Colon);

            var initialValue = ParseAssignmentExpression();

            var propertyNameDefinition = new PropertyNameDefinition(propertyName!, initialValue);

            propertyDefinitionList.Add(propertyNameDefinition);

            if (!_consumer.IsTokenOfType(TokenType.Comma))
            {
                break;
            }

            // NOTE: Trailing commas are allowed in property definition lists
            _consumer.ConsumeTokenOfType(TokenType.Comma);
        }

        return propertyDefinitionList;
    }

    private bool TryParsePropertyName(out INode? propertyName)
    {
        if (IsComputedProperty())
        {
            propertyName = ParseComputedPropertyName();
            return true;
        }
        else if (TryParseLiteralPropertyName(out propertyName))
        {
            return true;
        }

        propertyName = null;
        return false;
    }

    private ComputedPropertyName ParseComputedPropertyName()
    {
        _consumer.ConsumeTokenOfType(TokenType.OpenSquare);

        var expression = ParseAssignmentExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedSquare);

        return new ComputedPropertyName(expression);
    }

    private bool TryParseLiteralPropertyName(out INode? literalPropertyName)
    {
        INode literal;
        if (IsIdentifier())
        {
            literal = ParseIdentifier();
        }
        else if (IsStringLiteral())
        {
            literal = ParseStringLiteral();
        }
        else if (IsNumericLiteral())
        {
            literal = ParseNumericLiteral();
        }
        else
        {
            literalPropertyName = null;
            return false;
        }

        literalPropertyName = new LiteralPropertyName(literal);
        return true;
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

    private LetDeclaration ParseLetDeclaration(bool automaticSemicolonCandiate = true)
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

        ParseSemicolon(automaticSemicolonCandiate);

        return new LetDeclaration(identifierToken.Data, initializer);
    }

    private bool IsConstDeclaration()
    {
        return _consumer.IsTokenOfType(TokenType.Const);
    }

    private ConstDeclaration ParseConstDeclaration(bool automaticSemicolonCandiate = true)
    {
        _consumer.ConsumeTokenOfType(TokenType.Const);

        // FIXME: Allow await/yield to be used as an indentifier when specified in the spec
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        // FIXME: We should allow parsing multiple bindings in a single const declaration
        if (!_consumer.IsTokenOfType(TokenType.Assignment)) ErrorHelper.ThrowSyntaxError(ErrorType.ConstWithoutInitializer);

        var initializer = ParseInitializer();

        ParseSemicolon(automaticSemicolonCandiate);

        return new ConstDeclaration(identifierToken.Data, initializer);
    }

    // 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
    private bool IsVarStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Var);
    }

    private VarStatement ParseVarStatement(bool automaticSemicolonCandiate = true)
    {
        _consumer.ConsumeTokenOfType(TokenType.Var);

        var declarationList = ParseVarDeclarationList();

        ParseSemicolon(automaticSemicolonCandiate);

        return new VarStatement(declarationList);
    }

    private List<VarDeclaration> ParseVarDeclarationList()
    {
        List<VarDeclaration> declarationList = new();

        while (true)
        {
            var declaration = ParseVarDeclaration();
            declarationList.Add(declaration);

            // We want to keep parsing declarations every time there is a comma
            if (!_consumer.IsTokenOfType(TokenType.Comma))
            {
                break;
            }

            _consumer.ConsumeTokenOfType(TokenType.Comma);
        }

        return declarationList;
    }

    private VarDeclaration ParseVarDeclaration()
    {
        // FIXME: Allow let/await/yield to be used as an indentifier when specified in the spec
        var identifierToken = _consumer.ConsumeTokenOfType(TokenType.Identifier);

        INode? initializer = null;
        if (IsInitializer())
        {
            initializer = ParseInitializer();
        }

        return new(identifierToken.Data, initializer);
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
        if (IsAmbiguousForExpressionStatement())
        {
            parsedExpressionStatement = null;
            return false;
        }

        if (!TryParseExpression(out IExpression? expression))
        {
            parsedExpressionStatement = null;
            return false;
        }

        ParseSemicolon();

        parsedExpressionStatement = new ExpressionStatement(expression!);
        return true;
    }

    // For [lookahead ∉ { {, function, async [no LineTerminator here] function, class, let [ }]
    private bool IsAmbiguousForExpressionStatement()
    {
        return _consumer.CanConsume() && _consumer.Peek().Type switch
        {
            TokenType.OpenBrace or TokenType.Function or TokenType.Class => true,
            // FIXME: Check for async function
            // FIXME: This is a bit janky
            TokenType.Let => _consumer.CanConsume(1) && _consumer.Peek(1).Type == TokenType.OpenSquare,
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

        // NOTE: We only have to consume a semi-colon if there is one present due to automatic semi-colon insterion
        // The previous token is ) and the inserted semicolon would then be parsed as the terminating semicolon of a do-while statement (14.7.2).
        if (_consumer.IsTokenOfType(TokenType.SemiColon)) _consumer.ConsumeTokenOfType(TokenType.SemiColon);

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

    private INode ParseForStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.For);
        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var isNormalForLoop = IsNormalForLoop();

        if (isNormalForLoop)
        {
            return ParseNormalForLoop();
        }
        else
        {
            return ParseForInOfLoop();
        }

    }

    private bool IsNormalForLoop()
    {
        // If we find a semi-colon in the head of a for loop, that means it's a normal for loop
        // Otherwise, it is a for-in/for-of
        int offset = 0;
        int parenDepth = 1;
        while (_consumer.CanConsume(offset))
        {
            var current = _consumer.Peek(offset).Type;

            if (current == TokenType.OpenParen) ++parenDepth;
            if (current == TokenType.ClosedParen)
            {
                --parenDepth;
                if (parenDepth == 0) return false;
            }

            if (current == TokenType.SemiColon) return true;
            ++offset;
        }

        // NOTE: We have a syntax error here, but we'll let the other parsing code handle the syntax errors
        return true;
    }

    // 14.7.4 The for Statement, https://tc39.es/ecma262/#sec-for-statement
    private ForStatement ParseNormalForLoop()
    {
        TryParseForInitializationExpression(out INode? initializationExpression);

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
            _consumer.ConsumeTokenOfType(TokenType.SemiColon);
            initializationExpression = expression;
            return true;
        }
        if (IsVarStatement())
        {
            initializationExpression = ParseVarStatement(false);
            return true;
        }
        if (IsLetDeclaration())
        {
            initializationExpression = ParseLetDeclaration(false);
            return true;
        }
        if (IsConstDeclaration())
        {
            initializationExpression = ParseConstDeclaration(false);
            return true;
        }

        _consumer.ConsumeTokenOfType(TokenType.SemiColon);
        initializationExpression = null;
        return false;
    }

    // 14.7.5 The for-in, for-of, and for-await-of Statements, https://tc39.es/ecma262/#sec-for-in-and-for-of-statements
    private ForInStatement ParseForInOfLoop()
    {
        // FIXME: Early errors
        // FIXME: Implement parsing for the rest of the for-in, for-of and for-await-of statements
        _consumer.ConsumeTokenOfType(TokenType.Var);

        var identifier = ParseIdentifier();

        ParseIn();

        var expression = ParseExpression();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        var iterationStatement = ParseStatement();

        return new ForInStatement(identifier, expression, iterationStatement);
    }

    private void ParseIn()
    {
        var identifier = _consumer.Peek();

        if (identifier.Data != "in") ThrowUnexpectedTokenSyntaxError<ForInStatement>();

        _consumer.Consume();
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
        if (!_consumer.IsLineTerminator() && IsIdentifier())
        {
            label = ParseIdentifier();
        }

        ParseSemicolon();

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
        if (!_consumer.IsLineTerminator() && IsIdentifier())
        {
            label = ParseIdentifier();
        }

        ParseSemicolon();

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

        IExpression? returnExpression = null;
        if (!_consumer.IsLineTerminator()) TryParseExpression(out returnExpression);

        ParseSemicolon();

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

        var firstCaseBlocks = ParseCaseBlocks();

        var defaultBlock = ParseDefaultCase();

        var secondCaseBlocks = ParseCaseBlocks();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new SwitchStatement(switchExpression, firstCaseBlocks, secondCaseBlocks, defaultBlock);
    }

    private List<CaseBlock> ParseCaseBlocks()
    {
        List<CaseBlock> caseBlocks = new();
        while (IsCaseBlock())
        {
            caseBlocks.Add(ParseCaseBlock());
        }

        return caseBlocks;
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
            return _consumer.CanConsume() && _consumer.Peek().Type switch
            {
                TokenType.Case or TokenType.Default or TokenType.ClosedBrace => false,
                _ => true,
            };
        });
    }

    // 14.13 Labelled Statements, https://tc39.es/ecma262/#sec-labelled-statements
    private bool IsLabelledStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Identifier) && _consumer.IsTokenOfType(TokenType.Colon, 1);
    }

    private LabelledStatement ParseLabelledStatement()
    {
        var identifier = ParseIdentifier();

        _consumer.ConsumeTokenOfType(TokenType.Colon);

        var labelledItem = ParseLabelledItem();

        return new LabelledStatement(identifier, labelledItem);
    }

    private INode ParseLabelledItem()
    {
        if (IsFunctionDeclaration()) return ThrowUnexpectedTokenSyntaxError<INode>()!;
        return ParseStatement();
    }

    // 14.14 The throw Statement, https://tc39.es/ecma262/#sec-throw-statement
    private bool IsThrowStatement()
    {
        return _consumer.IsTokenOfType(TokenType.Throw);
    }

    private ThrowStatement ParseThrowStatement()
    {
        _consumer.ConsumeTokenOfType(TokenType.Throw);

        if (_consumer.IsLineTerminator()) ErrorHelper.ThrowSyntaxError(ErrorType.IllegalNewLineAfterThrow);

        var throwExpression = ParseExpression();

        ParseSemicolon();

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

        ParseSemicolon();

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

        var isStrict = ParseDirectivePrologue();
        var body = ParseFunctionBody();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new FunctionDeclaration(identifier.Name, parameters, body, isStrict);
    }

    private FunctionExpression ParseFunctionExpression()
    {
        _consumer.ConsumeTokenOfType(TokenType.Function);

        Identifier? identifier = IsIdentifier() ? ParseIdentifier() : null;

        _consumer.ConsumeTokenOfType(TokenType.OpenParen);

        var parameters = ParseFormalParameters();

        _consumer.ConsumeTokenOfType(TokenType.ClosedParen);

        _consumer.ConsumeTokenOfType(TokenType.OpenBrace);

        var isStrict = ParseDirectivePrologue();
        var body = ParseFunctionBody();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new FunctionExpression(identifier?.Name, parameters, body, isStrict);
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
        if (identifier.Data != "static") return false;

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
            return identifier.Data;
        }
        else if (_consumer.IsTokenOfType(TokenType.PrivateIdentifier))
        {
            var identifier = _consumer.Consume();
            return identifier.Data[1..];
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

        var isStrict = ParseDirectivePrologue();
        var body = ParseFunctionBody();

        _consumer.ConsumeTokenOfType(TokenType.ClosedBrace);

        return new MethodDeclaration(memberIdentifier, parameters, body, isPrivate, isStrict);
    }

    // 12.10.1 Rules of Automatic Semicolon Insertion, https://tc39.es/ecma262/multipage/ecmascript-language-lexical-grammar.html#sec-rules-of-automatic-semicolon-insertion
    private void ParseSemicolon(bool automaticSemicolonCandiate = true)
    {
        if (_consumer.IsTokenOfType(TokenType.SemiColon))
        {
            _consumer.ConsumeTokenOfType(TokenType.SemiColon);
            return;
        }

        if (automaticSemicolonCandiate && IsValidAutomaticSemicolonInsertionOffender())
        {
            return;
        }

        ThrowUnexpectedTokenSyntaxError<SyntaxErrorException>();
    }

    private bool IsValidAutomaticSemicolonInsertionOffender()
    {
        // The end of the input stream of tokens is encountered.
        if (!_consumer.CanConsume()) return true;

        // The offending token is }.
        if (_consumer.Peek().Type == TokenType.ClosedBrace) return true;

        // The offending token is separated from the previous token by at least one LineTerminator.
        // FIXME: Because of always ignoring line terminators, we have to do this hack,
        // we should probably just store the line count of tokens and use that to determine line terminators instead.
        return _consumer.IsLineTerminator() || (_consumer.CanConsume(-1) && _consumer.IsLineTerminator(-1));
    }

    // NOTE: We need to return a null for functions that return a value
    public T? ThrowUnexpectedTokenSyntaxError<T>()
    {
        if (_consumer.CanConsume())
        {
            var unexpectedToken = _consumer.Peek();
            ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedToken, unexpectedToken.Data);
        }

        ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedEOF);
        return default;
    }
}
