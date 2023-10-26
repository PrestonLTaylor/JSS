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

    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAnEmptyMultiLineComment()
    {
		// Arrange
		const string emptyCommentString = "/**/";
		var lexer = new Lexer(emptyCommentString);

		// Act
		var tokens = lexer.Lex().ToList();

		// Assert
		Assert.That(tokens, Is.Empty);
	}

	[Test]
	public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAMultiLineComment()
	{
		// Arrange
		const string commentString = "/* This is a multi-line comment */";
		var lexer = new Lexer(commentString);

		// Act
		var tokens = lexer.Lex().ToList();

		// Assert
		Assert.That(tokens, Is.Empty);
	}

    // FIXME: See FIXME in Lexer about Line Terminators
    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAMultiLineCommentWithLineTerminators()
    {
        // Arrange
        const string commentString = "/* This is a \n multi-line comment */";
        var lexer = new Lexer(commentString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    // Tests for 12.5 Hashbang Comments, https://tc39.es/ecma262/#sec-hashbang
    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAnHashBangComment()
    {
        // Arrange
        const string emptyCommentString = "#!";
        var lexer = new Lexer(emptyCommentString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    [Test]
    public void Lex_ReturnsEmptyListOfTokens_WhenProvidingAHashBangComment()
    {
        // Arrange
        const string commentString = "#! Hash Bang Comment";
        var lexer = new Lexer(commentString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Is.Empty);
    }

    [Test]
    public void Lex_ReturnsLineTerminatorToken_WhenProvidingAHashBangComment_WithALineTerminator()
    {
        // Arrange
        const string lineTerminatorCodePoint = "\u2029";
        const string commentWithLineTerminatorString = "#!" + lineTerminatorCodePoint;
        var lexer = new Lexer(commentWithLineTerminatorString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.LineTerminator, lineTerminatorCodePoint);
    }

    // Tests for 12.7.1 Identifier Names, https://tc39.es/ecma262/#sec-identifier-names
    [Test]
    public void Lex_ReturnsPrivateIdentifier_WhenProvidedHashAndLowerCaseLetter()
    {
        // Arrange
        const string lowerCaseLetter = "#a";
        var lexer = new Lexer(lowerCaseLetter);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.PrivateIdentifier, lowerCaseLetter);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedHashAndUpperCaseLetter()
    {
        // Arrange
        const string upperCaseLetter = "#A";
        var lexer = new Lexer(upperCaseLetter);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.PrivateIdentifier, upperCaseLetter);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedOnlyHashAndUnderscore()
    {
        // Arrange
        const string underscoreCodePoint = "#_";
        var lexer = new Lexer(underscoreCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.PrivateIdentifier, underscoreCodePoint);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedOnlyHashAndDollar()
    {
        // Arrange
        const string dollarCodePoint = "#$";
        var lexer = new Lexer(dollarCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.PrivateIdentifier, dollarCodePoint);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedHashAndLongValidName()
    {
        // Arrange
        const string validName = "#$_valid_name_123";
        var lexer = new Lexer(validName);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.PrivateIdentifier, validName);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedLowerCaseLetter()
    {
        // Arrange
        const string lowerCaseLetter = "a";
        var lexer = new Lexer(lowerCaseLetter);

        // Act
        var tokens = lexer.Lex().ToList();
        
        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, lowerCaseLetter);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedUpperCaseLetter()
    {
        // Arrange
        const string upperCaseLetter = "A";
        var lexer = new Lexer(upperCaseLetter);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, upperCaseLetter);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedOnlyAnUnderscore()
    {
        // Arrange
        const string underscoreCodePoint = "_";
        var lexer = new Lexer(underscoreCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, underscoreCodePoint);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedOnlyADollar()
    {
        // Arrange
        const string dollarCodePoint = "$";
        var lexer = new Lexer(dollarCodePoint);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, dollarCodePoint);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedALongValidName()
    {
        // Arrange
        const string validName = "$_valid_name_123";
        var lexer = new Lexer(validName);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, validName);
    }

    [Test]
    public void Lex_ReturnsIdentifier_WhenProvidedAKeywordWithAnExtraLetter()
    {
        // Arrange
        const string awaitKeywordWithEd = "awaited";
        var lexer = new Lexer(awaitKeywordWithEd);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Identifier, awaitKeywordWithEd);
    }

    // Tests for 12.7.2 Keywords and Reserved Words, https://tc39.es/ecma262/#sec-keywords-and-reserved-words
    static private readonly Dictionary<string, TokenType> reservedWordToTypeTestCases = new()
        {
            { "await", TokenType.Await },
            { "break", TokenType.Break },
            { "case", TokenType.Case },
            { "catch", TokenType.Catch },
            { "class", TokenType.Class },
            { "const", TokenType.Const },
            { "continue", TokenType.Continue },
            { "debugger", TokenType.Debugger },
            { "default", TokenType.Default },
            { "delete", TokenType.Delete },
            { "do", TokenType.Do },
            { "else", TokenType.Else },
            { "enum", TokenType.Enum },
            { "export", TokenType.Export },
            { "extends", TokenType.Extends },
            { "false", TokenType.False },
            { "finally", TokenType.Finally },
            { "for", TokenType.For },
            { "function", TokenType.Function },
            { "if", TokenType.If },
            { "import", TokenType.Import },
			// NOTE: We put instanceof due to wanting to match instanceof before in as they both have "in" in the as a common substring
            { "instanceof", TokenType.InstanceOf },
            { "in", TokenType.In },
            { "new", TokenType.New },
            { "null", TokenType.Null },
            { "return", TokenType.Return },
            { "super", TokenType.Super },
            { "switch", TokenType.Switch },
            { "this", TokenType.This },
            { "throw", TokenType.Throw },
            { "true", TokenType.True },
            { "try", TokenType.Try },
            { "typeof", TokenType.TypeOf },
            { "var", TokenType.Var },
            { "void", TokenType.Void },
            { "while", TokenType.While },
            { "with", TokenType.With },
            { "yield", TokenType.Yield },
        };

    [TestCaseSource(nameof(reservedWordToTypeTestCases))]
    public void Lex_ReturnsExpectedKeywordToken_WhenProvidingKeyword(KeyValuePair<string, TokenType> keywordToExpectedTokenType)
    {
        // Arrange
        var keywordString = keywordToExpectedTokenType.Key;
        var expectedTokenType = keywordToExpectedTokenType.Value;
        var lexer = new Lexer(keywordString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], expectedTokenType, keywordString);
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
