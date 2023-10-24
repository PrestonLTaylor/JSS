namespace JSS.Lib.UnitTests;

internal sealed class LexerTests
{
	[Test]
	public void Lex_ReturnsEmptyListOfTokens_WhenProvidedAnEmptyString()
	{
		// Arrange
		var lexer = new Lexer("");

		// Act
		var tokens = lexer.Lex().ToList();

		// Assert
		Assert.That(tokens, Is.Empty);
	}

    // Test for 12.2 White Space, https://tc39.es/ecma262/#sec-white-space
    [Test]
	public void Lex_ReturnsEmptyListOfTokens_WhenProvidingOnlyWhiteSpaceCodePoints()
	{
        // Arrange
        const string allWhiteSpaceCodePoints = "\u0009\u000B\u000C\uFEFF\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000";
		var lexer = new Lexer(allWhiteSpaceCodePoints);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

	// Tests for 12.3 Line Terminators, https://tc39.es/ecma262/#table-white-space-code-points
	[Test]
	public void Lex_ReturnsLineTerminatorToken_WhenProvidingALineFeed()
	{
		// Arrange
		const string lineFeedCodePoint = "\u000A";
		var lexer = new Lexer(lineFeedCodePoint);

		// Act
		var tokens = lexer.Lex().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, lineFeedCodePoint);
	}

    [Test]
    public void Lex_ReturnsLineTerminatorToken_WhenProvidingACarriageReturn()
    {
        // Arrange
        const string carriageReturnCodePoint = "\u000D";
        var lexer = new Lexer(carriageReturnCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, carriageReturnCodePoint);
    }

    [Test]
    public void Lex_ReturnsLineTerminatorToken_WhenProvidingALineSeperator()
    {
        // Arrange
        const string lineSeperatorCodePoint = "\u2028";
        var lexer = new Lexer(lineSeperatorCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, lineSeperatorCodePoint);
    }

    [Test]
    public void Lex_ReturnsLineTerminatorToken_WhenProvidingAParagraphSeperator()
    {
        // Arrange
        const string paragraphSeperatorCodePoint = "\u2029";
        var lexer = new Lexer(paragraphSeperatorCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, paragraphSeperatorCodePoint);
    }

    // Tests for 12.4 Comments, https://tc39.es/ecma262/#sec-comments
    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAnEmptySingleLineComment()
    {
        // Arrange
        const string emptyCommentString = "//";
        var lexer = new Lexer(emptyCommentString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingASingleLineComment()
    {
        // Arrange
        const string commentString = "// Single Line Comment";
        var lexer = new Lexer(commentString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    [Test]
    public void Lex_ReturnsLineTerminatorToken_WhenProvidingASingleLineComment_WithALineTerminator()
    {
        // Arrange
        const string lineTerminatorCodePoint = "\u2029";
        const string commentWithLineTerminatorString = "//" + lineTerminatorCodePoint;
        var lexer = new Lexer(commentWithLineTerminatorString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, lineTerminatorCodePoint);
    }

    private void AssertThatTokenIs(Token actual, TokenType expectedType, string expectedData)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.type, Is.EqualTo(expectedType));
            Assert.That(actual.data, Is.EqualTo(expectedData));
        });
    }
}
