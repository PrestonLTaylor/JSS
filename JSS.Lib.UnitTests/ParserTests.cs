using JSS.Lib.AST;
using JSS.Lib.AST.Literal;

namespace JSS.Lib.UnitTests;

internal sealed class ParserTests
{
    // Tests for LogicalORExpression, https://tc39.es/ecma262/#prod-LogicalORExpression
    [Test]
    public void Parse_ReturnsExpressionStatement_WithLogicalOrExpression_WhenProvidingLogicalOr()
    {
        // Arrange
        var parser = new Parser("1 || 2");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);
        Assert.That(expressionStatement.Expression as LogicalOrExpression, Is.Not.Null);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithNestedLogicalOrExpression_WhenProvidingMultipleLogicalOrs()
    {
        // Arrange
        var parser = new Parser("1 || 2 || 3");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        // FIXME: We want the LHS to be the LogicalOrExpression, as that is how the grammar in the spec is defined
        var outerOrExpression = expressionStatement.Expression as LogicalOrExpression;
        Assert.That(outerOrExpression, Is.Not.Null);
        Assert.That(outerOrExpression.Rhs as LogicalOrExpression, Is.Not.Null);
    }

    // Tests for 13.2 Primary Expression, https://tc39.es/ecma262/#sec-primary-expression
    [Test]
    public void Parse_ReturnsExpressionStatement_WithThisExpression_WhenProvidingThis()
    {
        // Arrange
        var parser = new Parser("this");

        // Act
        // FIXME: Decide if we should have a "IsExpressionStatement" kind of design as a virtual function inside of INode
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);
        Assert.That(expressionStatement.Expression as ThisExpression, Is.Not.Null);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithIdentifier_WhenProvidingValidIdentifier()
    {
        // Arrange
        const string identifierString = "validIdentifier";
        var parser = new Parser(identifierString);

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var identifier = expressionStatement.Expression as Identifier;
        Assert.That(identifier, Is.Not.Null);
        Assert.That(identifier.Name, Is.EqualTo(identifierString));
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithNullLiteral_WhenProvidingValidIdentifier()
    {
        // Arrange
        var parser = new Parser("null");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var expressionStatement = rootNodes[0] as ExpressionStatement;
        Assert.That(expressionStatement, Is.Not.Null);

        var nullLiteral = expressionStatement.Expression as NullLiteral;
        Assert.That(nullLiteral, Is.Not.Null);
    }

    [Test]
    public void Parse_ReturnsExpressionStatement_WithFalseBooleanLiteral_WhenProvidingFalseLiteral()
    {
        // Arrange
        var parser = new Parser("false");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var block = rootNodes[0] as Block;
        Assert.That(block, Is.Not.Null);
        Assert.That(block.Nodes, Is.Empty);
    }

    // FIXME: Make test cases for a variety of statements/declarations
    [Test]
    public void Parse_ReturnsABlock_WhenProvidingABlock()
    {
        // Arrange
        var parser = new Parser("{ 0 }");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var block = rootNodes[0] as Block;
        Assert.That(block, Is.Not.Null);

        var blockNodes = block.Nodes;
        Assert.That(blockNodes, Has.Count.EqualTo(1));
        Assert.That(blockNodes[0], Is.InstanceOf<ExpressionStatement>());
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingABlock_WithoutAClosingBrace()
    {
        // Arrange
        var parser = new Parser("{");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
    }

    static private readonly Dictionary<string, Type> initializerToExpectedTypeTestCases = new()
    {
        { "''", typeof(StringLiteral) },
        { "1", typeof(NumericLiteral) },
        { "false", typeof(BooleanLiteral) },
        { "null", typeof(NullLiteral) },
        { "otherIdentifier", typeof(Identifier) },
        { "this", typeof(ThisExpression) },
    };

    // Tests for 14.3.1 Let and Const Declarations, https://tc39.es/ecma262/#sec-let-and-const-declarations
    [Test]
    public void Parse_ReturnsLetDeclaration_WithNoInitializer_WhenProvidingLetDeclaration_WithNoInitializer()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"let {expectedIdentifier}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var letDeclaration = rootNodes[0] as LetDeclaration;
        Assert.That(letDeclaration, Is.Not.Null);
        Assert.That(letDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(letDeclaration.Initializer, Is.Null);
    }

    [TestCaseSource(nameof(initializerToExpectedTypeTestCases))]
    public void Parse_ReturnsLetDeclaration_WhenProvidingLetDeclaration(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"let {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var letDeclaration = rootNodes[0] as LetDeclaration;
        Assert.That(letDeclaration, Is.Not.Null);
        Assert.That(letDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(letDeclaration.Initializer, Is.InstanceOf(expectedInitializerType));
    }

    [TestCaseSource(nameof(initializerToExpectedTypeTestCases))]
    public void Parse_ReturnsConstDeclaration_WhenProvidingConstDeclaration(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"const {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var constDeclaration = rootNodes[0] as ConstDeclaration;
        Assert.That(constDeclaration, Is.Not.Null);
        Assert.That(constDeclaration.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(constDeclaration.Initializer, Is.InstanceOf(expectedInitializerType));
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingConstDeclartion_WithNoInitializer()
    {
        // Arrange
        var parser = new Parser("const a");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.Exception.TypeOf<InvalidOperationException>());
    }

    // Tests for 14.3.2 Variable Statement, https://tc39.es/ecma262/#sec-variable-statement
    [Test]
    public void Parse_ReturnsVarStatement_WithNoInitializer_WhenProvidingVarStatement_WithNoInitializer()
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var parser = new Parser($"var {expectedIdentifier}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var varStatement = rootNodes[0] as VarStatement;
        Assert.That(varStatement, Is.Not.Null);
        Assert.That(varStatement.Identifier, Is.EqualTo(expectedIdentifier));
        Assert.That(varStatement.Initializer, Is.Null);
    }

    [TestCaseSource(nameof(initializerToExpectedTypeTestCases))]
    public void Parse_ReturnsVarStatement_WhenProvidingVarStatement(KeyValuePair<string, Type> initializerToExpectedType)
    {
        // Arrange
        const string expectedIdentifier = "expectedIdentifier";
        var initializer = initializerToExpectedType.Key;
        var expectedInitializerType = initializerToExpectedType.Value;
        var parser = new Parser($"var {expectedIdentifier} = {initializer}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var emptyStatement = rootNodes[0] as EmptyStatement;
        Assert.That(emptyStatement, Is.Not.Null);
    }

    static private readonly Dictionary<string, Type> expressionToExpectedTypeTestCases = new()
    {
        { "''", typeof(StringLiteral) },
        { "1", typeof(NumericLiteral) },
        { "false", typeof(BooleanLiteral) },
        { "null", typeof(NullLiteral) },
        { "otherIdentifier", typeof(Identifier) },
        { "this", typeof(ThisExpression) },
    };

    // Tests for 14.6 The if Statement, https://tc39.es/ecma262/#sec-if-statement
    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsIfStatement_WithNoElse_WhenProvidingIf_WithNoElse(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"if ({expression}) {{ }}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var ifStatement = rootNodes[0] as IfStatement;
        Assert.That(ifStatement, Is.Not.Null);
        Assert.That(ifStatement.IfExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(ifStatement.IfCaseStatement, Is.Not.Null);
        Assert.That(ifStatement.ElseCaseStatement, Is.Not.Null);
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingIf_WithoutIfExpression()
    {
        // Arrange
        var parser = new Parser("if { }");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var doWhileStatement = rootNodes[0] as DoWhileStatement;
        Assert.That(doWhileStatement, Is.Not.Null);
        Assert.That(doWhileStatement.WhileExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(doWhileStatement.IterationStatement, Is.Not.Null);
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingDoWhile_WithNoExpression()
    {
        // Arrange
        var parser = new Parser("do { } while");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsWhileStatement_WhenProvidingWhile(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"while ({expression}) {{ }}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var whileStatement = rootNodes[0] as WhileStatement;
        Assert.That(whileStatement, Is.Not.Null);
        Assert.That(whileStatement.WhileExpression, Is.InstanceOf(expectedExpressionType));
        Assert.That(whileStatement.IterationStatement, Is.Not.Null);
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingWhile_WithNoExpression()
    {
        // Arrange
        var parser = new Parser("while { }");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
    }

    [TestCaseSource(nameof(expressionToExpectedTypeTestCases))]
    public void Parse_ReturnsForStatement_WhenProvidingFor(KeyValuePair<string, Type> expressionToExpectedType)
    {
        // Arrange
        var expression = expressionToExpectedType.Key;
        var expectedExpressionType = expressionToExpectedType.Value;
        var parser = new Parser($"for ({expression}; {expression}; {expression}) {{ }}");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var returnStatement = rootNodes[0] as ReturnStatement;
        Assert.That(returnStatement, Is.Not.Null);
        Assert.That(returnStatement.ReturnExpression, Is.InstanceOf(expectedExpressionType));
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var throwStatement = rootNodes[0] as ThrowStatement;
        Assert.That(throwStatement, Is.Not.Null);
        Assert.That(throwStatement.ThrowExpression, Is.InstanceOf(expectedExpressionType));
    }

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingThrow_WithNoExpression()
    {
        // Arrange
        var parser = new Parser("throw");

        // Act

        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
    }

    // Tests for 14.15 The try Statement, https://tc39.es/ecma262/#sec-try-statement
    [Test]
    public void Parse_ReturnsTryStatement_WithParameterlessCatch_WhenProvidingTry_WithParameterlessCatch()
    {
        // Arrange
        var parser = new Parser("try { } catch { }");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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

    [Test]
    public void Parse_ThrowsInvalidOperationException_WhenProvidingTry_WithoutCatchOrFinally()
    {
        // Arrange
        var parser = new Parser("try { }");

        // Act
        
        // Assert
        Assert.That(parser.Parse, Throws.InstanceOf<InvalidOperationException>());
    }

    // Tests for 14.16 The debugger Statement, https://tc39.es/ecma262/#sec-debugger-statement
    [Test]
    public void Parse_ReturnsDebuggerStatement_WhenProvidingDebugger()
    {
        // Arrange
        var parser = new Parser("debugger");

        // Act
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Parameters, Is.Empty);
        Assert.That(functionDeclaration.Body, Is.Empty);
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Body, Is.Empty);

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

        // Assert
        Assert.That(rootNodes, Has.Count.EqualTo(1));

        var functionDeclaration = rootNodes[0] as FunctionDeclaration;
        Assert.That(functionDeclaration, Is.Not.Null);
        Assert.That(functionDeclaration.Identifier, Is.EqualTo(expectedFunctionIdentifier));
        Assert.That(functionDeclaration.Body, Is.Empty);

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        Assert.That(publicMethod.Body, Is.Empty);
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        Assert.That(privateMethod.Body, Is.Empty);
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        Assert.That(staticPublicMethod.Body, Is.Empty);
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
        var parsedProgram = parser.Parse();
        var rootNodes = parsedProgram.RootNodes;

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
        Assert.That(staticPrivateMethod.Body, Is.Empty);
        Assert.That(staticPrivateMethod.IsPrivate, Is.True);
    }
}