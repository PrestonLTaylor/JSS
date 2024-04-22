using FluentAssertions;
using JSS.Lib.AST;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;

namespace JSS.Lib.UnitTests;

internal sealed class ParserTests
{
    static private readonly Dictionary<string, Type> expressionToExpectedTypeTestCases = new()
    {
        { "1 || 2", typeof(LogicalOrExpression) },
        { "1 && 2", typeof(LogicalAndExpression) },
        { "1 | 2", typeof(BitwiseOrExpression) },
        { "1 ^ 2", typeof(BitwiseXorExpression) },
        { "1 & 2", typeof(BitwiseAndExpression) },
        { "1 == 2", typeof(LooseEqualityExpression) },
        { "1 != 2", typeof(LooseInequalityExpression) },
        { "1 === 2", typeof(StrictEqualityExpression) },
        { "1 !== 2", typeof(StrictInequalityExpression) },
        { "1 < 2", typeof(LessThanExpression) },
        { "1 <= 2", typeof(LessThanEqualsExpression) },
        { "1 > 2", typeof(GreaterThanExpression) },
        { "1 >= 2", typeof(GreaterThanEqualsExpression) },
        { "1 instanceof 2", typeof(InstanceOfExpression) },
        { "1 in 2", typeof(InExpression) },
        { "1 << 2", typeof(LeftShiftExpression) },
        { "1 >> 2", typeof(RightShiftExpression) },
        { "1 >>> 2", typeof(UnsignedRightShiftExpression) },
        { "1 + 2", typeof(AdditionExpression) },
        { "1 - 2", typeof(SubtractionExpression) },
        { "1 * 2", typeof(MultiplicationExpression) },
        { "1 / 2", typeof(DivisionExpression) },
        { "1 % 2", typeof(ModuloExpression) },
        { "1 ** 2", typeof(ExponentiationExpression) },
        { "delete 1", typeof(DeleteExpression) },
        { "void 1", typeof(VoidExpression) },
        { "typeof 1", typeof(TypeOfExpression) },
        { "+1", typeof(UnaryPlusExpression) },
        { "-1", typeof(UnaryMinusExpression) },
        { "~1", typeof(BitwiseNotExpression) },
        { "!1", typeof(LogicalNotExpression) },
        { "1++", typeof(PostfixIncrementExpression) },
        { "1--", typeof(PostfixDecrementExpression) },
        { "++1", typeof(PrefixIncrementExpression) },
        { "--1", typeof(PrefixDecrementExpression) },
        { "new 1", typeof(NewExpression) },
        { "new 1(1)", typeof(NewExpression) },
        { "super.a", typeof(SuperPropertyExpression) },
        { "super[1]", typeof(SuperComputedPropertyExpression) },
        { "a.b", typeof(PropertyExpression) },
        { "a[1]", typeof(ComputedPropertyExpression) },
        { "a(1)", typeof(CallExpression) },
        { "this", typeof(ThisExpression) },
        { "(this)", typeof(ThisExpression) },
        { "null", typeof(NullLiteral) },
    };

    // Tests for Expression, https://tc39.es/ecma262/#prod-Expression
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsExpressionStatement_WithExpectedType_WhenProvidedExpessionStatement(KeyValuePair<string, Type> expressionStatementToType)
    {
        // Arrange
        var expressionStatement = expressionStatementToType.Key;
        var expectedType = expressionStatementToType.Value;
        var parser = new Parser(expressionStatement);

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var actualExpressionStatement = rootNodes[0] as ExpressionStatement;
        actualExpressionStatement.Should().NotBeNull();
        actualExpressionStatement!.Expression.Should().BeOfType(expectedType);
    }

    static private readonly Dictionary<string, Type> nestedExpressionStatementToTypeTestCases = new()
    {
        { "1 || 2 || 3", typeof(LogicalOrExpression) },
        { "1 && 2 && 3", typeof(LogicalAndExpression) },
        { "1 | 2 | 3", typeof(BitwiseOrExpression) },
        { "1 ^ 2 ^ 3", typeof(BitwiseXorExpression) },
        { "1 & 2 & 3", typeof(BitwiseAndExpression) },
        { "1 == 2 == 3", typeof(LooseEqualityExpression) },
        { "1 != 2 != 3", typeof(LooseInequalityExpression) },
        { "1 === 2 === 3", typeof(StrictEqualityExpression) },
        { "1 !== 2 !== 3", typeof(StrictInequalityExpression) },
        { "1 < 2 < 3", typeof(LessThanExpression) },
        { "1 <= 2 <= 3", typeof(LessThanEqualsExpression) },
        { "1 > 2 > 3", typeof(GreaterThanExpression) },
        { "1 >= 2 >= 3", typeof(GreaterThanEqualsExpression) },
        { "1 instanceof 2 instanceof 3", typeof(InstanceOfExpression) },
        { "1 in 2 in 3", typeof(InExpression) },
        { "1 << 2 << 3", typeof(LeftShiftExpression) },
        { "1 >> 2 >> 3", typeof(RightShiftExpression) },
        { "1 >>> 2 >>> 3", typeof(UnsignedRightShiftExpression) },
        { "1 + 2 + 3", typeof(AdditionExpression) },
        { "1 - 2 - 3", typeof(SubtractionExpression) },
        { "1 * 2 * 3", typeof(MultiplicationExpression) },
        { "1 / 2 / 3", typeof(DivisionExpression) },
        { "1 % 2 % 3", typeof(ModuloExpression) },
        { "1 ** 2 ** 3", typeof(ExponentiationExpression) },
        { "delete delete 1", typeof(DeleteExpression) },
        { "void void 1", typeof(VoidExpression) },
        { "typeof typeof 1", typeof(TypeOfExpression) },
        { "~~1", typeof(BitwiseNotExpression) },
        { "!!1", typeof(LogicalNotExpression) },
        { "new new 1(1)", typeof(NewExpression) },
        { "a.b.c", typeof(PropertyExpression) },
        { "a[1][2]", typeof(ComputedPropertyExpression) },
        { "a(1)(2)", typeof(CallExpression) },
        { "((this))", typeof(ThisExpression) },
    };

    [TestCaseSource(nameof(nestedExpressionStatementToTypeTestCases))]
    public void Parse_ReturnsExpressionStatement_WithExpectedType_WhenProvidedNestedExpessionStatement(KeyValuePair<string, Type> expressionStatementToType)
    {
        // Arrange
        var expressionStatement = expressionStatementToType.Key;
        var expectedType = expressionStatementToType.Value;
        var parser = new Parser(expressionStatement);

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var actualExpressionStatement = rootNodes[0] as ExpressionStatement;
        actualExpressionStatement.Should().NotBeNull();
        actualExpressionStatement!.Expression.Should().BeOfType(expectedType);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithIdentifier_WhenProvidingValidIdentifier()
    {
        // Arrange
        const string identifierString = "validIdentifier";
        var parser = new Parser(identifierString);

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var identifier = expressionStatement!.Expression as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(identifierString);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithFalseBooleanLiteral_WhenProvidingFalseLiteral()
    {
        // Arrange
        var parser = new Parser("false");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var booleanLiteral = expressionStatement!.Expression as BooleanLiteral;
        booleanLiteral.Should().NotBeNull();
        booleanLiteral!.Value.Should().BeFalse();
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithTrueBooleanLiteral_WhenProvidingTrueLiteral()
    {
        // Arrange
        var parser = new Parser("true");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var booleanLiteral = expressionStatement!.Expression as BooleanLiteral;
        booleanLiteral.Should().NotBeNull();
        booleanLiteral!.Value.Should().BeTrue();
    }

    static private readonly Dictionary<string, double> numericLiteralToValueTestCases = new()
    {
        { "0", 0.0 }, { "1", 1.0 }, { "123", 123.0 }, { "1234567890", 1234567890.0 }
    };

    [TestCaseSource(nameof(numericLiteralToValueTestCases))]
    public void Parse_ReturnsExpressionStatement_WithNumericLiteral_WhenProvidingNumericLiteral(KeyValuePair<string, double> numericLiteralToValue)
    {
        // Arrange
        var numericLiteral = numericLiteralToValue.Key;
        var numericValue = numericLiteralToValue.Value;
        var parser = new Parser(numericLiteral);

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var parsedLiteral = expressionStatement!.Expression as NumericLiteral;
        parsedLiteral.Should().NotBeNull();
        parsedLiteral!.Value.Should().BeApproximately(numericValue, 0.0001);
    }

    static private readonly Dictionary<string, string> stringLiteralToValueTestCases = new()
    {
        { "\"\"", "" },
        { "''", "" },
        { "\"this is a string literal\"", "this is a string literal" },
        { "'this is a string literal'", "this is a string literal" },
        { "\"'\"", "'" },
        { "'\"'", "\"" }
    };

    [TestCaseSource(nameof(stringLiteralToValueTestCases))]
    public void Parse_ReturnsExpressionStatement_WithStringLiteral_WhenProvidingStringLiteral(KeyValuePair<string, string> stringLiteralToValue)
    {
        // Arrange
        var stringLiteral = stringLiteralToValue.Key;
        var stringValue = stringLiteralToValue.Value;
        var parser = new Parser(stringLiteral);

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var parsedLiteral = expressionStatement!.Expression as StringLiteral;
        parsedLiteral.Should().NotBeNull();
        parsedLiteral!.Value.Should().Be(stringValue);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithNewExpressionWithArguments_WhenProvidingANewWithArguments()
    {
        // Arrange
        var parser = new Parser("new Object({})");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var newExpression = expressionStatement!.Expression as NewExpression;
        newExpression.Should().NotBeNull();
        newExpression!.Arguments.Should().HaveCount(1);
    }

    // Tests for 14.2 Block, https://tc39.es/ecma262/#sec-block
    [Test]
    public void Parse_ReturnsAnEmptyBlock_WhenProvidingAnEmptyBlock()
    {
        // Arrange
        var parser = new Parser("{}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var block = rootNodes[0] as Block;
        block.Should().NotBeNull();
        block!.Statements.Statements.Should().BeEmpty();
    }

    // FIXME: Make test cases for a variety of statements/declarations
    [Test]
    public void Parse_ReturnsABlock_WhenProvidingABlock()
    {
        // Arrange
        var parser = new Parser("{ 0 }");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var block = rootNodes[0] as Block;
        block.Should().NotBeNull();

        var blockNodes = block!.Statements.Statements;
        blockNodes.Should().HaveCount(1);
        blockNodes[0].Should().BeOfType<ExpressionStatement>();
    }

    // Tests for 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
    [Test]
    public void Parse_ReturnsLetDeclaration_WithNoInitializer_WhenProvidingLetDeclaration_WithNoInitializer()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"let {expectedIdentifier}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var letDeclaration = rootNodes[0] as LetDeclaration;
        letDeclaration.Should().NotBeNull();
        letDeclaration!.Identifier.Should().Be(expectedIdentifier);
        letDeclaration.Initializer.Should().BeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsLetDeclaration_WhenProvidingLetDeclaration(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"let {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var letDeclaration = rootNodes[0] as LetDeclaration;
        letDeclaration.Should().NotBeNull();
        letDeclaration!.Identifier.Should().Be(expectedIdentifier);
        letDeclaration.Initializer.Should().BeOfType(expectedInitializerType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsConstDeclaration_WhenProvidingConstDeclaration(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"const {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var constDeclaration = rootNodes[0] as ConstDeclaration;
        constDeclaration.Should().NotBeNull();
        constDeclaration!.Identifier.Should().Be(expectedIdentifier);
        constDeclaration.Initializer.Should().BeOfType(expectedInitializerType);
    }

    // Tests for 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
    [Test]
    public void Parse_ReturnsVarStatement_WithNoInitializer_WhenProvidingVarStatement_WithNoInitializer()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"var {expectedIdentifier}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var varStatement = rootNodes[0] as VarStatement;
        varStatement.Should().NotBeNull();
        varStatement!.Declarations.Should().HaveCount(1);

        var varDeclaration = varStatement.Declarations[0];
        varDeclaration.Identifier.Should().Be(expectedIdentifier);
        varDeclaration.Initializer.Should().BeNull();
    }

    [Test]
    public void Parse_ReturnsVarStatement_WithMultipleDeclarations_WhenProvidingVarStatement_WithMultipleDeclarations()
    {
        // Arrange
        const string expectedFirstIdentifier = "expectedFirstIdentifier";
        const string expectedSecondIdentifier = "expectedSecondIdentifier";
        var parser = new Parser($"var {expectedFirstIdentifier}, {expectedSecondIdentifier}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var varStatement = rootNodes[0] as VarStatement;
        varStatement.Should().NotBeNull();
        varStatement!.Declarations.Should().HaveCount(2);

        varStatement.Declarations[0].Identifier.Should().Be(expectedFirstIdentifier);
        varStatement.Declarations[1].Identifier.Should().Be(expectedSecondIdentifier);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsVarStatement_WhenProvidingVarStatement(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"var {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var varStatement = rootNodes[0] as VarStatement;
        varStatement.Should().NotBeNull();
        varStatement!.Declarations.Should().HaveCount(1);

        var varDeclaration = varStatement.Declarations[0];
        varDeclaration.Identifier.Should().Be(expectedIdentifier);
        varDeclaration.Initializer.Should().BeOfType(expectedInitializerType);
    }

    // Tests for 14.4 Empty Statement, https://tc39.es/ecma262/#sec-empty-statement
    [Test]
    public void Parse_ReturnsEmptyStatement_WhenProvidingEmptyStatement()
    {
        // Arrange
        var parser = new Parser(";");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var emptyStatement = rootNodes[0] as EmptyStatement;
        emptyStatement.Should().NotBeNull();
    }

    // Tests for 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsIfStatement_WithNoElse_WhenProvidingIf_WithNoElse(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"if ({expression}) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var ifStatement = rootNodes[0] as IfStatement;
        ifStatement.Should().NotBeNull();
        ifStatement!.IfExpression.Should().BeOfType(expectedExpressionType);
        ifStatement.IfCaseStatement.Should().NotBeNull();
        ifStatement.ElseCaseStatement.Should().BeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsIfStatement_WhenProvidingIf(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"if ({expression}) {{ }} else {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var ifStatement = rootNodes[0] as IfStatement;
        ifStatement.Should().NotBeNull();
        ifStatement!.IfExpression.Should().BeOfType(expectedExpressionType);
        ifStatement.IfCaseStatement.Should().NotBeNull();
        ifStatement.ElseCaseStatement.Should().NotBeNull();
    }

    // Tests for 14.7 Iteration Statements, https://tc39.es/ecma262/#sec-iteration-statements
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsDoWhileStatement_WhenProvidingDoWhile(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"do {{ }} while ({expression})");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var doWhileStatement = rootNodes[0] as DoWhileStatement;
        doWhileStatement.Should().NotBeNull();
        doWhileStatement!.WhileExpression.Should().BeOfType(expectedExpressionType);
        doWhileStatement.IterationStatement.Should().NotBeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsWhileStatement_WhenProvidingWhile(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"while ({expression}) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var whileStatement = rootNodes[0] as WhileStatement;
        whileStatement.Should().NotBeNull();
        whileStatement!.WhileExpression.Should().BeOfType(expectedExpressionType);
        whileStatement.IterationStatement.Should().NotBeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsForStatement_WhenProvidingFor(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"for ({expression}; {expression}; {expression}) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var forStatement = rootNodes[0] as ForStatement;
        forStatement.Should().NotBeNull();
        forStatement!.InitializationExpression.Should().BeOfType(expectedExpressionType);
        forStatement.TestExpression.Should().BeOfType(expectedExpressionType);
        forStatement.IncrementExpression.Should().BeOfType(expectedExpressionType);
        forStatement.IterationStatement.Should().NotBeNull();
    }

    [Test]
    public void Parse_ReturnsForStatement_WhenProvidingFor_WithNoExpressions()
    {
        // Arrange
        var parser = new Parser($"for (;;) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var forStatement = rootNodes[0] as ForStatement;
        forStatement.Should().NotBeNull();
        forStatement!.InitializationExpression.Should().BeNull();
        forStatement.TestExpression.Should().BeNull();
        forStatement.IncrementExpression.Should().BeNull();
        forStatement.IterationStatement.Should().NotBeNull();
    }

    [Test]
    public void Parse_ReturnsForStatement_WhenProvidingFor_WithVariableDeclaration()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"for (var {expectedIdentifier};;) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var forStatement = rootNodes[0] as ForStatement;
        forStatement.Should().NotBeNull();
        forStatement!.TestExpression.Should().BeNull();
        forStatement.IncrementExpression.Should().BeNull();
        forStatement.IterationStatement.Should().NotBeNull();

        var varStatement = forStatement.InitializationExpression as VarStatement;
        varStatement.Should().NotBeNull();
        varStatement!.Declarations.Should().HaveCount(1);

        var varDeclaration = varStatement.Declarations[0];
        varDeclaration.Identifier.Should().Be(expectedIdentifier);
    }

    [Test]
    public void Parse_ReturnsForStatement_WhenProvidingFor_WithLetDeclaration()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"for (let {expectedIdentifier};;) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var forStatement = rootNodes[0] as ForStatement;
        forStatement.Should().NotBeNull();
        forStatement!.TestExpression.Should().BeNull();
        forStatement.IncrementExpression.Should().BeNull();
        forStatement.IterationStatement.Should().NotBeNull();

        var varStatement = forStatement.InitializationExpression as LetDeclaration;
        varStatement.Should().NotBeNull();
        varStatement!.Identifier.Should().Be(expectedIdentifier);
    }

    [Test]
    public void Parse_ReturnsForStatement_WhenProvidingFor_WithConstDeclaration()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"for (const {expectedIdentifier} = 0;;) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var forStatement = rootNodes[0] as ForStatement;
        forStatement.Should().NotBeNull();
        forStatement!.TestExpression.Should().BeNull();
        forStatement.IncrementExpression.Should().BeNull();
        forStatement.IterationStatement.Should().NotBeNull();

        var varStatement = forStatement.InitializationExpression as ConstDeclaration;
        varStatement.Should().NotBeNull();
        varStatement!.Identifier.Should().Be(expectedIdentifier);
    }

    // Tests for 14.8 The continue Statement, https://tc39.es/ecma262/#sec-continue-statement
    [Test]
    public void Parse_ReturnsContinueStatement_WhenProvidingContinue()
    {
        // Arrange
        const string expectedLabel = "expectedLabel";
        var parser = new Parser($"continue {expectedLabel}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var continueStatement = rootNodes[0] as ContinueStatement;
        continueStatement.Should().NotBeNull();

        continueStatement!.Label.Should().NotBeNull();
        continueStatement.Label!.Name.Should().Be(expectedLabel);
    }

    [Test]
    public void Parse_ReturnsContinueStatement_WithoutLabel_WhenProvidingContinue_WithoutLabel()
    {
        // Arrange
        var parser = new Parser("continue");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var continueStatement = rootNodes[0] as ContinueStatement;
        continueStatement.Should().NotBeNull();
        continueStatement!.Label.Should().BeNull();
    }

    // Tests for 14.9 The break Statement, https://tc39.es/ecma262/#sec-break-statement
    [Test]
    public void Parse_ReturnsBreakStatement_WhenProvidingBreak()
    {
        // Arrange
        const string expectedLabel = "expectedLabel";
        var parser = new Parser($"break {expectedLabel}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var breakStatement = rootNodes[0] as BreakStatement;
        breakStatement.Should().NotBeNull();

        breakStatement!.Label.Should().NotBeNull();
        breakStatement.Label!.Name.Should().Be(expectedLabel);
    }

    [Test]
    public void Parse_ReturnsBreakStatement_WithoutLabel_WhenProvidingBreak_WithoutLabel()
    {
        // Arrange
        var parser = new Parser("break");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var breakStatement = rootNodes[0] as BreakStatement;
        breakStatement.Should().NotBeNull();
        breakStatement!.Label.Should().BeNull();
    }

    // Tests for 14.10 The return Statement, https://tc39.es/ecma262/#sec-return-statement
    [Test]
    public void Parse_ReturnsReturnStatement_WithNoExpression_WhenProvidingReturn_WithNoExpression()
    {
        // Arrange
        var parser = new Parser("return");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var returnStatement = rootNodes[0] as ReturnStatement;
        returnStatement.Should().NotBeNull();
        returnStatement!.ReturnExpression.Should().BeNull();
    }

    [Test]
    public void Parse_ReturnsReturnStatement_WithNoExpression_WhenProvidingReturn_WithNewLineThenExpression()
    {
        // Arrange
        var parser = new Parser("return\n1");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(2);

        var returnStatement = rootNodes[0] as ReturnStatement;
        returnStatement.Should().NotBeNull();
        returnStatement!.ReturnExpression.Should().BeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsReturnStatement_WhenProvidingReturn(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"return {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var returnStatement = rootNodes[0] as ReturnStatement;
        returnStatement.Should().NotBeNull();
        returnStatement!.ReturnExpression.Should().BeOfType(expectedExpressionType);
    }

    // Tests for 14.12 The switch Statement, https://tc39.es/ecma262/#sec-switch-statement
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsSwitchStatement_WithEmptyCaseBlock_WhenProvidingSwitch_WithEmptyCaseBlock(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"switch ({expression}) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var switchStatement = rootNodes[0] as SwitchStatement;
        switchStatement.Should().NotBeNull();
        switchStatement!.SwitchExpression.Should().BeOfType(expectedExpressionType);
        switchStatement.CaseBlocks.Should().BeEmpty();
        switchStatement.DefaultCase.Should().BeNull();
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsSwitchStatement_WithADefaultCase_WhenProvidingSwitch_WithADefaultCase(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"switch ({expression}) {{ default: {expression} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var switchStatement = rootNodes[0] as SwitchStatement;
        switchStatement.Should().NotBeNull();
        switchStatement!.SwitchExpression.Should().BeOfType(expectedExpressionType);
        switchStatement.CaseBlocks.Should().BeEmpty();
        switchStatement.DefaultCase.Should().NotBeNull();

        var defaultCase = switchStatement.DefaultCase!.Value;
        defaultCase.StatementList.Should().HaveCount(1);

        var defaultCaseStatement = defaultCase.StatementList[0] as ExpressionStatement;
        defaultCaseStatement.Should().NotBeNull();
        defaultCaseStatement!.Expression.Should().BeOfType(expectedExpressionType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsSwitchStatement_WithACaseBlock_WhenProvidingSwitch_WithACaseBlock(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"switch ({expression}) {{ case {expression}: {expression} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var switchStatement = rootNodes[0] as SwitchStatement;
        switchStatement.Should().NotBeNull();
        switchStatement!.SwitchExpression.Should().BeOfType(expectedExpressionType);
        switchStatement.DefaultCase.Should().BeNull();
        switchStatement.CaseBlocks.Should().HaveCount(1);

        var caseBlock = switchStatement.CaseBlocks[0];
        caseBlock.CaseExpression.Should().BeOfType(expectedExpressionType);
        caseBlock.StatementList.Should().HaveCount(1);

        var caseStatement = caseBlock.StatementList[0] as ExpressionStatement;
        caseStatement.Should().NotBeNull();
        caseStatement!.Expression.Should().BeOfType(expectedExpressionType);
    }

    [Test]
    public void Parse_ReturnsSwitchStatement_WithADefaultAndCaseBlocks_WhenProvidingSwitch_WithADefaultAndCaseBlocks()
    {
        // Arrange
        var parser = new Parser("switch (1) { case 2: 3 default: 4 case 5: 6 }");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var switchStatement = rootNodes[0] as SwitchStatement;
        switchStatement.Should().NotBeNull();
        switchStatement!.SwitchExpression.Should().NotBeNull();
        switchStatement.DefaultCase.Should().NotBeNull();
        switchStatement.CaseBlocks.Should().HaveCount(2);
    }
    
    // Tests for 14.14 The throw Statement, https://tc39.es/ecma262/#sec-throw-statement
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsThrowStatement_WhenProvidingThrow(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"throw {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var throwStatement = rootNodes[0] as ThrowStatement;
        throwStatement.Should().NotBeNull();
        throwStatement!.ThrowExpression.Should().BeOfType(expectedExpressionType);
    }

    [Test]
    public void Parse_ThrowsSyntaxError_WhenProvidingThrow_WithNewLineBeforeExpression()
    {
        // Arrange
        var parser = new Parser($"throw\n1");
        var expectedError = ErrorHelper.CreateSyntaxError(ErrorType.IllegalNewLineAfterThrow);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedError);
    }


    // Tests for 14.15 The try Statement, https://tc39.es/ecma262/#sec-try-statement
    [Test]
    public void Parse_ReturnsTryStatement_WithParameterlessCatch_WhenProvidingTry_WithParameterlessCatch()
    {
        // Arrange
        var parser = new Parser("try { } catch { }");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var tryStatement = rootNodes[0] as TryStatement;
        tryStatement.Should().NotBeNull();
        tryStatement!.TryBlock.Should().NotBeNull();
        tryStatement.CatchBlock.Should().NotBeNull();
        tryStatement.CatchParameter.Should().BeNull();
        tryStatement.FinallyBlock.Should().BeNull();
    }

    // FIXME: Tests for BindingPatterns
    [Test]
    public void Parse_ReturnsTryStatement_WithCatch_WhenProvidingTry_WithCatch()
    {
        // Arrange
        const string expectedCatchIdentifier = "expectedIdentifier";
        var parser = new Parser($"try {{ }} catch ({expectedCatchIdentifier}) {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var tryStatement = rootNodes[0] as TryStatement;
        tryStatement.Should().NotBeNull();
        tryStatement!.TryBlock.Should().NotBeNull();
        tryStatement.CatchBlock.Should().NotBeNull();
        tryStatement.CatchParameter.Should().NotBeNull();
        tryStatement.CatchParameter!.Name.Should().Be(expectedCatchIdentifier);
        tryStatement.FinallyBlock.Should().BeNull();
    }

    [Test]
    public void Parse_ReturnsTryStatement_WithFinally_WhenProvidingTry_WithFinally()
    {
        // Arrange
        var parser = new Parser("try { } finally { }");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var tryStatement = rootNodes[0] as TryStatement;
        tryStatement.Should().NotBeNull();
        tryStatement!.TryBlock.Should().NotBeNull();
        tryStatement.CatchBlock.Should().BeNull();
        tryStatement.CatchParameter.Should().BeNull();
        tryStatement.FinallyBlock.Should().NotBeNull();
    }

    [Test]
    public void Parse_ReturnsTryStatement_WithParameterlessCatchFinally_WhenProvidingTry_WithParameterlessCatchFinally()
    {
        // Arrange
        var parser = new Parser("try { } catch { } finally { }");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var tryStatement = rootNodes[0] as TryStatement;
        tryStatement.Should().NotBeNull();
        tryStatement!.TryBlock.Should().NotBeNull();
        tryStatement.CatchBlock.Should().NotBeNull();
        tryStatement.CatchParameter.Should().BeNull();
        tryStatement.FinallyBlock.Should().NotBeNull();
    }

    [Test]
    public void Parse_ReturnsTryStatement_WithCatchFinally_WhenProvidingTry_WithCatchFinally()
    {
        // Arrange
        const string expectedCatchIdentifier = "expectedIdentifier";
        var parser = new Parser($"try {{ }} catch ({expectedCatchIdentifier}) {{ }} finally {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var tryStatement = rootNodes[0] as TryStatement;
        tryStatement.Should().NotBeNull();
        tryStatement!.TryBlock.Should().NotBeNull();
        tryStatement.CatchBlock.Should().NotBeNull();
        tryStatement.CatchParameter.Should().NotBeNull();
        tryStatement.CatchParameter!.Name.Should().Be(expectedCatchIdentifier);
        tryStatement.FinallyBlock.Should().NotBeNull();
    }

    // Tests for 14.16 The debugger Statement, https://tc39.es/ecma262/#sec-debugger-statement
    [Test]
    public void Parse_ReturnsDebuggerStatement_WhenProvidingDebugger()
    {
        // Arrange
        var parser = new Parser("debugger");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var debuggerStatement = rootNodes[0] as DebuggerStatement;
        debuggerStatement.Should().NotBeNull();
    }

    // Tests for 15.2 Function Definitions, https://tc39.es/ecma262/#sec-function-definitions
    [Test]
    public void Parse_ReturnsFunctionDeclaration_WithNoParameters_WhenProvidingFunctionDeclaration_WithNoParameters()
    {
        // Arrange
        const string expectedFunctionIdentifier = "expectedIdentifier";
        var parser = new Parser($"function {expectedFunctionIdentifier}() {{}}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        functionDeclaration.Should().NotBeNull();
        functionDeclaration!.Identifier.Should().Be(expectedFunctionIdentifier);
        functionDeclaration.Parameters.Should().BeEmpty();
        functionDeclaration.Body.Statements.Should().BeEmpty();
    }

    [Test]
    public void Parse_ReturnsFunctionDeclaration_WhenProvidingFunctionDeclaration()
    {
        // Arrange
        const string expectedFunctionIdentifier = "expectedIdentifier";
        const string expectedFirstParameterIdentifier = "expectedFirstParameter";
        const string expectedSecondParameterIdentifier = "expectedSecondParameter";
        var parser = new Parser($"function {expectedFunctionIdentifier}({expectedFirstParameterIdentifier}, {expectedSecondParameterIdentifier}) {{}}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        functionDeclaration.Should().NotBeNull();
        functionDeclaration!.Identifier.Should().Be(expectedFunctionIdentifier);
        functionDeclaration.Body.Statements.Should().BeEmpty();

        var parameters  = functionDeclaration.Parameters;
        parameters.Should().HaveCount(2);
        parameters[0].Name.Should().Be(expectedFirstParameterIdentifier);
        parameters[1].Name.Should().Be(expectedSecondParameterIdentifier);
    }

    [Test]
    public void Parse_ReturnsFunctionDeclaration_WhenProvidingFunctionDeclaration_WithTrailingComma_InParameterList()
    {
        // Arrange
        const string expectedFunctionIdentifier = "expectedIdentifier";
        const string expectedParameterIdentifier = "expectedFirstParameter";
        var parser = new Parser($"function {expectedFunctionIdentifier}({expectedParameterIdentifier},) {{}}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        functionDeclaration.Should().NotBeNull();
        functionDeclaration!.Identifier.Should().Be(expectedFunctionIdentifier);
        functionDeclaration.Body.Statements.Should().BeEmpty();

        var parameters = functionDeclaration.Parameters;
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be(expectedParameterIdentifier);
    }

    // NOTE: Function expressions cannot be expression statements, so we have to also have an a node that parses an expression
    [Test]
    public void Parse_ReturnsAssignmentExpression_WithFunctionExpression_WhenProvidingAssignmentWithAFunctionExpression()
    {
        // Arrange
        var parser = new Parser($"a = function() {{}}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BasicAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var functionExpression = assignmentExpression!.Rhs as FunctionExpression;
        functionExpression.Should().NotBeNull();
        functionExpression!.Identifier.Should().BeNull();
        functionExpression.Body.Statements.Should().BeEmpty();

        var parameters = functionExpression.Parameters;
        parameters.Should().BeEmpty();
    }

    [Test]
    public void Parse_ReturnsAssignmentExpression_WithFunctionExpression_WithAName_WhenProvidingAssignmentWithAFunctionExpression_WithAName()
    {
        // Arrange
        const string expectedFunctionIdentifier = "expectedIdentifier";
        const string expectedParameterIdentifier = "expectedFirstParameter";
        var parser = new Parser($"a = function {expectedFunctionIdentifier}({expectedParameterIdentifier}) {{}}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BasicAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var functionExpression = assignmentExpression!.Rhs as FunctionExpression;
        functionExpression.Should().NotBeNull();
        functionExpression!.Identifier.Should().Be(expectedFunctionIdentifier);
        functionExpression.Body.Statements.Should().BeEmpty();

        var parameters = functionExpression.Parameters;
        parameters.Should().HaveCount(1);
        parameters[0].Name.Should().Be(expectedParameterIdentifier);
    }

    // Tests for 15.7 Class Definitions, https://tc39.es/ecma262/#sec-class-definitions
    [Test]
    public void Parse_ReturnsEmptyClassDeclaration_WhenProvidingEmptyClass()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"class {expectedIdentifier} {{ }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        classDeclaration.Should().NotBeNull();
        classDeclaration!.Identifier.Should().Be(expectedIdentifier);
        classDeclaration.Methods.Should().BeEmpty();
        classDeclaration.StaticMethods.Should().BeEmpty();
        classDeclaration.Fields.Should().BeEmpty();
        classDeclaration.StaticFields.Should().BeEmpty();
    }

    [Test]
    public void Parse_ReturnsClassDeclaration_WithPublicMethod_WhenProvidingClass_WithPublicMethod()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        const string expectedMethodIdentifier = "expectedMethodIdentifier";
        var parser = new Parser($"class {expectedIdentifier} {{ {expectedMethodIdentifier}() {{ }} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        classDeclaration.Should().NotBeNull();
        classDeclaration!.Identifier.Should().Be(expectedIdentifier);
        classDeclaration.StaticMethods.Should().BeEmpty();
        classDeclaration.Fields.Should().BeEmpty();
        classDeclaration.StaticFields.Should().BeEmpty();

        classDeclaration.Methods.Should().HaveCount(1);

        var publicMethod = classDeclaration.Methods[0];
        publicMethod.Identifier.Should().Be(expectedMethodIdentifier);
        publicMethod.Parameters.Should().BeEmpty();
        publicMethod.Body.Statements.Should().BeEmpty();
        publicMethod.IsPrivate.Should().BeFalse();
    }

    [Test]
    public void Parse_ReturnsClassDeclaration_WithPrivateMethod_WhenProvidingClass_WithPrivateMethod()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        const string expectedMethodIdentifier = "expectedMethodIdentifier";
        var parser = new Parser($"class {expectedIdentifier} {{ #{expectedMethodIdentifier}() {{ }} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        classDeclaration.Should().NotBeNull();
        classDeclaration!.Identifier.Should().Be(expectedIdentifier);
        classDeclaration.StaticMethods.Should().BeEmpty();
        classDeclaration.Fields.Should().BeEmpty();
        classDeclaration.StaticFields.Should().BeEmpty();

        classDeclaration.Methods.Should().HaveCount(1);

        var privateMethod = classDeclaration.Methods[0];
        privateMethod.Identifier.Should().Be(expectedMethodIdentifier);
        privateMethod.Parameters.Should().BeEmpty();
        privateMethod.Body.Statements.Should().BeEmpty();
        privateMethod.IsPrivate.Should().BeTrue();
    }

    [Test]
    public void Parse_ReturnsClassDeclaration_WithStaticPublicMethod_WhenProvidingClass_WithStaticPublicMethod()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        const string expectedMethodIdentifier = "expectedMethodIdentifier";
        var parser = new Parser($"class {expectedIdentifier} {{ static {expectedMethodIdentifier}() {{ }} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        classDeclaration.Should().NotBeNull();
        classDeclaration!.Identifier.Should().Be(expectedIdentifier);
        classDeclaration.Methods.Should().BeEmpty();
        classDeclaration.Fields.Should().BeEmpty();
        classDeclaration.StaticFields.Should().BeEmpty();

        classDeclaration.StaticMethods.Should().HaveCount(1);

        var staticPublicMethod = classDeclaration.StaticMethods[0];
        staticPublicMethod.Identifier.Should().Be(expectedMethodIdentifier);
        staticPublicMethod.Parameters.Should().BeEmpty();
        staticPublicMethod.Body.Statements.Should().BeEmpty();
        staticPublicMethod.IsPrivate.Should().BeFalse();
    }

    [Test]
    public void Parse_ReturnsClassDeclaration_WithPrivateMethod_WhenProvidingClass_WithStaticPrivateMethod()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        const string expectedMethodIdentifier = "expectedMethodIdentifier";
        var parser = new Parser($"class {expectedIdentifier} {{ static #{expectedMethodIdentifier}() {{ }} }}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        classDeclaration.Should().NotBeNull();
        classDeclaration!.Identifier.Should().Be(expectedIdentifier);
        classDeclaration.Methods.Should().BeEmpty();
        classDeclaration.Fields.Should().BeEmpty();
        classDeclaration.StaticFields.Should().BeEmpty();

        classDeclaration.StaticMethods.Should().HaveCount(1);

        var staticPrivateMethod = classDeclaration.StaticMethods[0];
        staticPrivateMethod.Identifier.Should().Be(expectedMethodIdentifier);
        staticPrivateMethod.Parameters.Should().BeEmpty();
        staticPrivateMethod.Body.Statements.Should().BeEmpty();
        staticPrivateMethod.IsPrivate.Should().BeTrue();
    }

    // Tests for AssignmentExpressions
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBasicAssignmentExpression_WhenProvidingBasicAssignmentExpression(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "identifier";
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"{expectedIdentifier} = {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BasicAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var identifier = assignmentExpression!.Lhs as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(expectedIdentifier);

        assignmentExpression.Rhs.Should().BeOfType(expectedExpressionType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsLogicalAndAssignmentExpression_WhenProvidingLogicalAndAssignmentExpression(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "identifier";
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"{expectedIdentifier} &&= {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as LogicalAndAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var identifier = assignmentExpression!.Lhs as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(expectedIdentifier);

        assignmentExpression.Rhs.Should().BeOfType(expectedExpressionType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsLogicalOrAssignmentExpression_WhenProvidingLogicalOrAssignmentExpression(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "identifier";
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"{expectedIdentifier} ||= {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as LogicalOrAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var identifier = assignmentExpression!.Lhs as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(expectedIdentifier);

        assignmentExpression.Rhs.Should().BeOfType(expectedExpressionType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsNullCoalescingAssignmentExpression_WhenProvidingNullCoalescingAssignmentExpression(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "identifier";
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"{expectedIdentifier} ??= {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as NullCoalescingAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var identifier = assignmentExpression!.Lhs as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(expectedIdentifier);

        assignmentExpression.Rhs.Should().BeOfType(expectedExpressionType);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithExponentiationBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "**=", BinaryOpType.Exponentiate);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithMultiplyBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "*=", BinaryOpType.Multiply);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithDivideBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "/=", BinaryOpType.Divide);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithRemainderBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "%=", BinaryOpType.Remainder);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithAddBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "+=", BinaryOpType.Add);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithSubtractBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "-=", BinaryOpType.Subtract);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithLeftShiftBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "<<=", BinaryOpType.LeftShift);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithSignedRightShiftBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, ">>=", BinaryOpType.SignedRightShift);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithUnsignedRightShiftBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, ">>>=", BinaryOpType.UnsignedRightShift);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithBitwiseANDBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "&=", BinaryOpType.BitwiseAND);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithBitwiseXORBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "^=", BinaryOpType.BitwiseXOR);
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsBinaryOpAssignmentExpression_WithBitwiseORBinaryOp(KeyValuePair<string, Type> expressionToExpectedType)
    {
        Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(expressionToExpectedType, "|=", BinaryOpType.BitwiseOR);
    }

    private void Parse_ReturnsBinaryOpAssignmentExpression_WithExpectedBinaryOp(KeyValuePair<string, Type> expressionToExpectedType, string binaryOp, BinaryOpType expectedOp)
    {
        // Arrange
        const string expectedIdentifier = "identifier";
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"{expectedIdentifier} {binaryOp} {expression}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BinaryOpAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var identifier = assignmentExpression!.Lhs as Identifier;
        identifier.Should().NotBeNull();
        identifier!.Name.Should().Be(expectedIdentifier);

        assignmentExpression.Op.Should().Be(expectedOp);

        assignmentExpression.Rhs.Should().BeOfType(expectedExpressionType);
    }

    // FIXME: More tests when we don't only parse empty array literals
    // Tests for ArrayLiteral
    [Test]
    public void Parse_ReturnsAssignment_WithArrayLiteralRHS_WhenProvidingAssignment_WithArrayLiteralRHS()
    {
        // Arrange
        var parser = new Parser("a = []");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BasicAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var objectLiteral = assignmentExpression!.Rhs as ArrayLiteral;
        objectLiteral.Should().NotBeNull();
    }

    // FIXME: More tests when we don't only parse empty object literals
    // Tests for ObjectLiteral
    [Test]
    public void Parse_ReturnsAssignment_WithObjectLiteralRHS_WhenProvidingAssignment_WithObjectLiteralRHS()
    {
        // Arrange
        var parser = new Parser("a = {}");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        rootNodes.Should().HaveCount(1);

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        expressionStatement.Should().NotBeNull();

        var assignmentExpression = expressionStatement!.Expression as BasicAssignmentExpression;
        assignmentExpression.Should().NotBeNull();

        var objectLiteral = assignmentExpression!.Rhs as ObjectLiteral;
        objectLiteral.Should().NotBeNull();
    }

    // Tests for SyntaxErrors
    static private readonly Dictionary<string, string> unexpectedTokenTestCases = new()
    {
        {"", "}"},
        {"1||", "}"},
        {"1&&", "}"},
        {"1|", "}"},
        {"1^", "}"},
        {"1&", "}"},
        {"1==", "}"},
        {"1===", "}"},
        {"1!=", "}"},
        {"1!==", "}"},
        {"1<", "}"},
        {"1>", "}"},
        {"1<=", "}"},
        {"1>=", "}"},
        {"1 instanceof", "}"},
        {"1 in", "}"},
        {"1<<", "}"},
        {"1>>", "}"},
        {"1>>>", "}"},
        {"1+", "}"},
        {"1-", "}"},
        {"1*", "}"},
        {"1/", "}"},
        {"1%", "}"},
        {"1**", "}"},
        {"++", "}"},
        {"--", "}"},
        {"new", "}"},
        {"new a(", "}"},
        {"a(", "}"},
        {"(", "}"},
        {"(1 ", "1"},
        {"a(1, ", "const"},
        {"const a =", "}"},
        {"if", "}"},
        {"if (1", "}"},
        {"while (", "}"},
        {"while", "}"},
        {"do", "}"},
        {"do { } while", "}"},
        {"do { } while (", "}"},
        {"a[", "}"},
        {"a[1", "}"},
        {"a.", "}"},
        {"super[", "}"},
        {"super[1", "}"},
        {"super.", "}"},
        {"let", "}"},
        {"let a = ", "}"},
        {"var a,", "}" },
        {"for (1 ", "1"},
        {"for (1; 1 ", "1"},
        {"for (1; 1; 1 ", "1"},
        {"switch", "}"},
        {"switch (", "}"},
        {"switch (1", "}"},
        {"switch (1)", "}"},
        {"switch (1) {", "1"},
        {"switch (1) { case", "}"},
        {"switch (1) { case 1", "}"},
        {"switch (1) { default", "}"},
        {"throw", "}"},
        {"function", "}"},
        {"function a", "}"},
        {"function a(", "}"},
        {"function a(b ", "c"},
        {"function a()", "}"},
        {"class", "}"},
        {"class a {", "1"},
        {"class a { b(", "}"},
        {"class a { b(c ", "d"},
        {"class a { b(c) ", "}"},
        {"a ", "}"},
        {"a =", "}"},
        {"a &&=", "}"},
        {"a ||=", "}"},
        {"a ??=", "}"},
        {"a **=", "}"},
        {"a *=", "}"},
        {"a /=", "}"},
        {"a %=", "}"},
        {"a +=", "}"},
        {"a -=", "}"},
        {"a <<=", "}"},
        {"a >>=", "}"},
        {"a >>>=", "}"},
        {"a &=", "}"},
        {"a ^=", "}"},
        {"a |=", "}"},
    };

    [TestCaseSource(nameof(unexpectedTokenTestCases))]
    public void Parse_ThrowsUnexpectedTokenSyntaxErorr_WhenProvidingAnUnexpectedToken(KeyValuePair<string, string> validToInvalidTestCase)
    {
        // Arrange
        var parser = new Parser($"{validToInvalidTestCase.Key}{validToInvalidTestCase.Value}");
        var expectedException = ErrorHelper.CreateSyntaxError(ErrorType.UnexpectedToken, validToInvalidTestCase.Value);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedException);
    }

    static private readonly List<string> unexpectedEofTestCases = new()
    {
        "throw",
        "{",
        "(",
        "1||",
        "1&&",
        "1|",
        "1^",
        "1&",
        "1==",
        "1===",
        "1!=",
        "1!==",
        "1<",
        "1>",
        "1<=",
        "1>=",
        "1 instanceof",
        "1 in",
        "1<<",
        "1>>",
        "1>>>",
        "1+",
        "1-",
        "1*",
        "1/",
        "1%",
        "1**",
        "++",
        "--",
        "new",
        "new a(",
        "a(",
        "(",
        "(1 ",
        "a(1, ",
        "const a =",
        "if",
        "if (1",
        "while (",
        "while",
        "do",
        "do { } while",
        "do { } while (",
        "a[",
        "a[1",
        "a.",
        "super[",
        "super[1",
        "super.",
        "let",
        "let a = ",
        "let a = {",
        "var a,",
        "for (1 ",
        "for (1; 1 ",
        "for (1; 1; 1 ",
        "switch",
        "switch (",
        "switch (1",
        "switch (1)",
        "switch (1) {",
        "switch (1) { case",
        "switch (1) { case 1",
        "switch (1) { case 1: 1",
        "switch (1) { default",
        "throw",
        "function",
        "function a",
        "function a(",
        "function a(b ",
        "function a()",
        "class",
        "class a {",
        "class a { b(",
        "class a { b(c ",
        "class a { b(c) ",
        "a =",
        "a &&=",
        "a ||=",
        "a ??=",
        "a **=",
        "a *=",
        "a /=",
        "a %=",
        "a +=",
        "a -=",
        "a <<=",
        "a >>=",
        "a >>>=",
        "a &=",
        "a ^=",
        "a |=",
    };

    [TestCaseSource(nameof(unexpectedEofTestCases))]
    public void Parse_ThrowsUnexpectedEOFSyntaxError_WhenProvidingInput_WithUnexpectedEOF(string testCase)
    {
        // Arrange
        var parser = new Parser(testCase);
        var expectedException = ErrorHelper.CreateSyntaxError(ErrorType.UnexpectedEOF);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedException);
    }

    [Test]
    public void Parse_ThrowsUnaryLHSOfExponentiationSyntaxError_WhenProvidingUnaryLHS_OfExponentiationExpression()
    {
        // Arrange
        var parser = new Parser("!1**2");
        var expectedException = ErrorHelper.CreateSyntaxError(ErrorType.UnaryLHSOfExponentiation);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedException);
    }

    [Test]
    public void Parse_ThrowsConstWithoutInitializerSyntaxError_WhenConstDeclaration_WithoutInitializerSyntaxError()
    {
        // Arrange
        var parser = new Parser("const a");
        var expectedException = ErrorHelper.CreateSyntaxError(ErrorType.ConstWithoutInitializer);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedException);
    }

    [Test]
    public void Parse_ThrowsTryWithoutCatchOrFianllySyntaxError_WhenProvidingTryWithoutCatchOrFianlly()
    {
        // Arrange
        var parser = new Parser("try { }");
        var expectedException = ErrorHelper.CreateSyntaxError(ErrorType.TryWithoutCatchOrFinally);

        // Act

        // Assert
        AssertThatSyntaxErrorMatchesExpected(parser, expectedException);
    }

    private void AssertThatSyntaxErrorMatchesExpected(Parser parser, SyntaxErrorException expectedException)
    {
        var actualException = Assert.Throws<SyntaxErrorException>(() => ParseScript(parser));
        actualException.Message.Should().Be(expectedException.Message);
    }

    static private Script ParseScript(Parser parser)
    {
        var completion = Realm.InitializeHostDefinedRealm(out VM vm);
        if (completion.IsAbruptCompletion())
        {
            Assert.Fail("Failed to initialize host defined realm");
        }

        return parser.Parse(vm);
    }
}