﻿using JSS.Lib.AST;
using JSS.Lib.AST.Literal;

namespace JSS.Lib.UnitTests;

internal sealed class ParserTests
{
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
}
