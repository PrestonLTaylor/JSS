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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var actualExpressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(actualExpressionStatement, Is.Not.Null);
        Assert.That(actualExpressionStatement.Expression, Is.InstanceOf(expectedType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var actualExpressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(actualExpressionStatement, Is.Not.Null);
        Assert.That(actualExpressionStatement.Expression, Is.InstanceOf(expectedType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var identifier = expressionStatement.Expression as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(identifierString));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var booleanLiteral = expressionStatement.Expression as BooleanLiteral;
        Assert.That(booleanLiteral, Is.Not.Null);
        Assert.That(booleanLiteral.Value, Is.EqualTo(false));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var booleanLiteral = expressionStatement.Expression as BooleanLiteral;
        Assert.That(booleanLiteral, Is.Not.Null);
        Assert.That(booleanLiteral.Value, Is.EqualTo(true));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var parsedLiteral = expressionStatement.Expression as NumericLiteral;
        Assert.That(parsedLiteral, Is.Not.Null);
        Assert.That(parsedLiteral.Value, Is.EqualTo(numericValue));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var parsedLiteral = expressionStatement.Expression as StringLiteral;
        Assert.That(parsedLiteral, Is.Not.Null);
        Assert.That(parsedLiteral.Value, Is.EqualTo(stringValue));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var block = rootNodes[0] as Block;
        Assert.That(block, Is.Not.Null);
        Assert.That(block.Statements.Statements, Is.Empty);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var block = rootNodes[0] as Block;
        Assert.That(block, Is.Not.Null);

        var blockNodes = block.Statements.Statements;
        Assert.That(blockNodes, Has.Count.EqualTo(1));
        Assert.That(blockNodes[0], Is.InstanceOf<ExpressionStatement>());
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var letDeclaration = rootNodes[0] as LetDeclaration;
        Assert.That(letDeclaration, Is.Not.Null);
        Assert.That(letDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(letDeclaration.Initializer, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var letDeclaration = rootNodes[0] as LetDeclaration;
        Assert.That(letDeclaration, Is.Not.Null);
        Assert.That(letDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(letDeclaration.Initializer, Is.InstanceOf(expectedInitializerType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var constDeclaration = rootNodes[0] as ConstDeclaration;
        Assert.That(constDeclaration, Is.Not.Null);
        Assert.That(constDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(constDeclaration.Initializer, Is.InstanceOf(expectedInitializerType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var varStatement = rootNodes[0] as VarStatement;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(varStatement.Initializer, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var varStatement = rootNodes[0] as VarStatement;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(varStatement.Initializer, Is.InstanceOf(expectedInitializerType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var emptyStatement = rootNodes[0] as EmptyStatement;
        Assert.That(emptyStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var ifStatement = rootNodes[0] as IfStatement;
        Assert.That(ifStatement, Is.Not.Null);
        Assert.That(ifStatement.IfExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(ifStatement.IfCaseStatement, Is.Not.Null);
        Assert.That(ifStatement.ElseCaseStatement, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var ifStatement = rootNodes[0] as IfStatement;
        Assert.That(ifStatement, Is.Not.Null);
        Assert.That(ifStatement.IfExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(ifStatement.IfCaseStatement, Is.Not.Null);
        Assert.That(ifStatement.ElseCaseStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var doWhileStatement = rootNodes[0] as DoWhileStatement;
        Assert.That(doWhileStatement, Is.Not.Null);
        Assert.That(doWhileStatement.WhileExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(doWhileStatement.IterationStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var whileStatement = rootNodes[0] as WhileStatement;
        Assert.That(whileStatement, Is.Not.Null);
        Assert.That(whileStatement.WhileExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(whileStatement.IterationStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var forStatement = rootNodes[0] as ForStatement;
        Assert.That(forStatement, Is.Not.Null);
        Assert.That(forStatement.InitializationExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(forStatement.TestExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(forStatement.IncrementExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(forStatement.IterationStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var forStatement = rootNodes[0] as ForStatement;
        Assert.That(forStatement, Is.Not.Null);
        Assert.That(forStatement.InitializationExpression, Is.Null);
        Assert.That(forStatement.TestExpression, Is.Null);
        Assert.That(forStatement.IncrementExpression, Is.Null);
        Assert.That(forStatement.IterationStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var forStatement = rootNodes[0] as ForStatement;
        Assert.That(forStatement, Is.Not.Null);
        Assert.That(forStatement.TestExpression, Is.Null);
        Assert.That(forStatement.IncrementExpression, Is.Null);
        Assert.That(forStatement.IterationStatement, Is.Not.Null);

        var varStatement = forStatement.InitializationExpression as VarStatement;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var forStatement = rootNodes[0] as ForStatement;
        Assert.That(forStatement, Is.Not.Null);
        Assert.That(forStatement.TestExpression, Is.Null);
        Assert.That(forStatement.IncrementExpression, Is.Null);
        Assert.That(forStatement.IterationStatement, Is.Not.Null);

        var varStatement = forStatement.InitializationExpression as LetDeclaration;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var forStatement = rootNodes[0] as ForStatement;
        Assert.That(forStatement, Is.Not.Null);
        Assert.That(forStatement.TestExpression, Is.Null);
        Assert.That(forStatement.IncrementExpression, Is.Null);
        Assert.That(forStatement.IterationStatement, Is.Not.Null);

        var varStatement = forStatement.InitializationExpression as ConstDeclaration;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var continueStatement = rootNodes[0] as ContinueStatement;
        Assert.That(continueStatement, Is.Not.Null);
        Assert.That(continueStatement.Label, Is.Not.Null);
        Assert.That(continueStatement.Label.Name, Is.EqualTo(expectedLabel));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var continueStatement = rootNodes[0] as ContinueStatement;
        Assert.That(continueStatement, Is.Not.Null);
        Assert.That(continueStatement.Label, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var breakStatement = rootNodes[0] as BreakStatement;
        Assert.That(breakStatement, Is.Not.Null);
        Assert.That(breakStatement.Label, Is.Not.Null);
        Assert.That(breakStatement.Label.Name, Is.EqualTo(expectedLabel));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var breakStatement = rootNodes[0] as BreakStatement;
        Assert.That(breakStatement, Is.Not.Null);
        Assert.That(breakStatement.Label, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var returnStatement = rootNodes[0] as ReturnStatement;
        Assert.That(returnStatement, Is.Not.Null);
        Assert.That(returnStatement.ReturnExpression, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var returnStatement = rootNodes[0] as ReturnStatement;
        Assert.That(returnStatement, Is.Not.Null);
        Assert.That(returnStatement.ReturnExpression, Is.InstanceOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var switchStatement = rootNodes[0] as SwitchStatement;
        Assert.That(switchStatement, Is.Not.Null);
        Assert.That(switchStatement.SwitchExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(switchStatement.CaseBlocks, Is.Empty);
        Assert.That(switchStatement.DefaultCase, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var switchStatement = rootNodes[0] as SwitchStatement;
        Assert.That(switchStatement, Is.Not.Null);
        Assert.That(switchStatement.SwitchExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(switchStatement.CaseBlocks, Is.Empty);
        Assert.That(switchStatement.DefaultCase, Is.Not.Null);

        var defaultCase = switchStatement.DefaultCase!.Value;
        Assert.That(defaultCase.StatementList, Has.Count.EqualTo(1));

        var defaultCaseStatement = defaultCase.StatementList[0] as ExpressionStatement;
        Assert.That(defaultCaseStatement, Is.Not.Null);
        Assert.That(defaultCaseStatement.Expression, Is.InstanceOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var switchStatement = rootNodes[0] as SwitchStatement;
        Assert.That(switchStatement, Is.Not.Null);
        Assert.That(switchStatement.SwitchExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(switchStatement.DefaultCase, Is.Null);
        Assert.That(switchStatement.CaseBlocks, Has.Count.EqualTo(1));

        var caseBlock = switchStatement.CaseBlocks[0];
        Assert.That(caseBlock.CaseExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(caseBlock.StatementList, Has.Count.EqualTo(1));

        var caseStatement = caseBlock.StatementList[0] as ExpressionStatement;
        Assert.That(caseStatement, Is.Not.Null);
        Assert.That(caseStatement.Expression, Is.InstanceOf(expectedExpressionType));
    }

    public void Parse_ReturnsSwitchStatement_WithADefaultAndCaseBlocks_WhenProvidingSwitch_WithADefaultAndCaseBlocks()
    {
        // Arrange
        var parser = new Parser("switch (1) { case 2: 3 default: 4 case 5: 6");

        // Act
        var parsedProgram = ParseScript(parser);
        var rootNodes = parsedProgram.ScriptCode;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var switchStatement = rootNodes[0] as SwitchStatement;
        Assert.That(switchStatement, Is.Not.Null);
        Assert.That(switchStatement.SwitchExpression, Is.Not.Null);
        Assert.That(switchStatement.DefaultCase, Is.Not.Null);
        Assert.That(switchStatement.CaseBlocks, Has.Count.EqualTo(2));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var throwStatement = rootNodes[0] as ThrowStatement;
        Assert.That(throwStatement, Is.Not.Null);
        Assert.That(throwStatement.ThrowExpression, Is.InstanceOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var tryStatement = rootNodes[0] as TryStatement;
        Assert.That(tryStatement, Is.Not.Null);
        Assert.That(tryStatement.TryBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter, Is.Null);
        Assert.That(tryStatement.FinallyBlock, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var tryStatement = rootNodes[0] as TryStatement;
        Assert.That(tryStatement, Is.Not.Null);
        Assert.That(tryStatement.TryBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter!.Name, Is.EqualTo(expectedCatchIdentifier));
        Assert.That(tryStatement.FinallyBlock, Is.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var tryStatement = rootNodes[0] as TryStatement;
        Assert.That(tryStatement, Is.Not.Null);
        Assert.That(tryStatement.TryBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchBlock, Is.Null);
        Assert.That(tryStatement.CatchParameter, Is.Null);
        Assert.That(tryStatement.FinallyBlock, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var tryStatement = rootNodes[0] as TryStatement;
        Assert.That(tryStatement, Is.Not.Null);
        Assert.That(tryStatement.TryBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter, Is.Null);
        Assert.That(tryStatement.FinallyBlock, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var tryStatement = rootNodes[0] as TryStatement;
        Assert.That(tryStatement, Is.Not.Null);
        Assert.That(tryStatement.TryBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchBlock, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter, Is.Not.Null);
        Assert.That(tryStatement.CatchParameter!.Name, Is.EqualTo(expectedCatchIdentifier));
        Assert.That(tryStatement.FinallyBlock, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var debuggerStatement = rootNodes[0] as DebuggerStatement;
        Assert.That(debuggerStatement, Is.Not.Null);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Parameters, Is.Empty);
        Assert.That(functionDeclaration.Body.Statements, Is.Empty);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Body.Statements, Is.Empty);

        var parameters  = functionDeclaration.Parameters;
        Assert.That(parameters, Has.Count.EqualTo(2));
        Assert.That(parameters[0].Name, Is.EqualTo(expectedFirstParameterIdentifier));
        Assert.That(parameters[1].Name, Is.EqualTo(expectedSecondParameterIdentifier));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Body.Statements, Is.Empty);

        var parameters = functionDeclaration.Parameters;
        Assert.That(parameters, Has.Count.EqualTo(1));
        Assert.That(parameters[0].Name, Is.EqualTo(expectedParameterIdentifier));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        Assert.That(classDeclaration, Is.Not.Null);
        Assert.That(classDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(classDeclaration.Methods, Is.Empty);
        Assert.That(classDeclaration.StaticMethods, Is.Empty);
        Assert.That(classDeclaration.Fields, Is.Empty);
        Assert.That(classDeclaration.StaticFields, Is.Empty);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        Assert.That(classDeclaration, Is.Not.Null);
        Assert.That(classDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(classDeclaration.StaticMethods, Is.Empty);
        Assert.That(classDeclaration.Fields, Is.Empty);
        Assert.That(classDeclaration.StaticFields, Is.Empty);

        Assert.That(classDeclaration.Methods, Has.Count.EqualTo(1));

        var publicMethod = classDeclaration.Methods[0];
        Assert.That(publicMethod.Identifier, Is.EqualTo(expectedMethodIdentifier));
        Assert.That(publicMethod.Parameters, Is.Empty);
        Assert.That(publicMethod.Body.Statements, Is.Empty);
        Assert.That(publicMethod.IsPrivate, Is.False);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        Assert.That(classDeclaration, Is.Not.Null);
        Assert.That(classDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(classDeclaration.StaticMethods, Is.Empty);
        Assert.That(classDeclaration.Fields, Is.Empty);
        Assert.That(classDeclaration.StaticFields, Is.Empty);

        Assert.That(classDeclaration.Methods, Has.Count.EqualTo(1));

        var privateMethod = classDeclaration.Methods[0];
        Assert.That(privateMethod.Identifier, Is.EqualTo(expectedMethodIdentifier));
        Assert.That(privateMethod.Parameters, Is.Empty);
        Assert.That(privateMethod.Body.Statements, Is.Empty);
        Assert.That(privateMethod.IsPrivate, Is.True);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        Assert.That(classDeclaration, Is.Not.Null);
        Assert.That(classDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(classDeclaration.Methods, Is.Empty);
        Assert.That(classDeclaration.Fields, Is.Empty);
        Assert.That(classDeclaration.StaticFields, Is.Empty);

        Assert.That(classDeclaration.StaticMethods, Has.Count.EqualTo(1));

        var staticPublicMethod = classDeclaration.StaticMethods[0];
        Assert.That(staticPublicMethod.Identifier, Is.EqualTo(expectedMethodIdentifier));
        Assert.That(staticPublicMethod.Parameters, Is.Empty);
        Assert.That(staticPublicMethod.Body.Statements, Is.Empty);
        Assert.That(staticPublicMethod.IsPrivate, Is.False);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var classDeclaration = rootNodes[0] as ClassDeclaration;
        Assert.That(classDeclaration, Is.Not.Null);
        Assert.That(classDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(classDeclaration.Methods, Is.Empty);
        Assert.That(classDeclaration.Fields, Is.Empty);
        Assert.That(classDeclaration.StaticFields, Is.Empty);

        Assert.That(classDeclaration.StaticMethods, Has.Count.EqualTo(1));

        var staticPrivateMethod = classDeclaration.StaticMethods[0];
        Assert.That(staticPrivateMethod.Identifier, Is.EqualTo(expectedMethodIdentifier));
        Assert.That(staticPrivateMethod.Parameters, Is.Empty);
        Assert.That(staticPrivateMethod.Body.Statements, Is.Empty);
        Assert.That(staticPrivateMethod.IsPrivate, Is.True);
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as BasicAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var identifier = assignmentExpression.Lhs as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(expectedIdentifier));

        Assert.That(assignmentExpression.Rhs, Is.TypeOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as LogicalAndAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var identifier = assignmentExpression.Lhs as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(expectedIdentifier));

        Assert.That(assignmentExpression.Rhs, Is.TypeOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as LogicalOrAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var identifier = assignmentExpression.Lhs as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(expectedIdentifier));

        Assert.That(assignmentExpression.Rhs, Is.TypeOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as NullCoalescingAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var identifier = assignmentExpression.Lhs as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(expectedIdentifier));

        Assert.That(assignmentExpression.Rhs, Is.TypeOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as BinaryOpAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var identifier = assignmentExpression.Lhs as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(expectedIdentifier));

        Assert.That(assignmentExpression.Op, Is.EqualTo(expectedOp));

        Assert.That(assignmentExpression.Rhs, Is.TypeOf(expectedExpressionType));
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
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var assignmentExpression = expressionStatement.Expression as BasicAssignmentExpression;
        Assert.That(assignmentExpression, Is.Not.Null);

        var objectLiteral = assignmentExpression.Rhs as ObjectLiteral;
        Assert.That(objectLiteral, Is.Not.Null);
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
        Assert.That(actualException.Message, Is.EqualTo(expectedException.Message));
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