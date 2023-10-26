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
    [Test]
    public void Lex_ReturnsAwaitToken_WhenProvidingAwait()
    {
        // Arrange
        const string awaitString = "await";
        var lexer = new Lexer(awaitString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Await, awaitString);
    }

    [Test]
    public void Lex_ReturnsBreakToken_WhenProvidingBreak()
    {
        // Arrange
        const string breakString = "break";
        var lexer = new Lexer(breakString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Break, breakString);
    }

    [Test]
    public void Lex_ReturnsCaseToken_WhenProvidingCase()
    {
        // Arrange
        const string caseString = "case";
        var lexer = new Lexer(caseString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Case, caseString);
    }

    [Test]
    public void Lex_ReturnsCatchToken_WhenProvidingCatch()
    {
        // Arrange
        const string catchString = "catch";
        var lexer = new Lexer(catchString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Catch, catchString);
    }

    [Test]
    public void Lex_ReturnsClassToken_WhenProvidingClass()
    {
        // Arrange
        const string classString = "class";
        var lexer = new Lexer(classString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Class, classString);
    }

    [Test]
    public void Lex_ReturnsConstToken_WhenProvidingConst()
    {
        // Arrange
        const string constString = "const";
        var lexer = new Lexer(constString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Const, constString);
    }

    [Test]
    public void Lex_ReturnsContinueToken_WhenProvidingContinue()
    {
        // Arrange
        const string continueString = "continue";
        var lexer = new Lexer(continueString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Continue, continueString);
    }

    [Test]
    public void Lex_ReturnsDebuggerToken_WhenProvidingDebugger()
    {
        // Arrange
        const string debuggerString = "debugger";
        var lexer = new Lexer(debuggerString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Debugger, debuggerString);
    }

    [Test]
    public void Lex_ReturnsDefaultToken_WhenProvidingDefault()
    {
        // Arrange
        const string defaultString = "default";
        var lexer = new Lexer(defaultString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Default, defaultString);
    }

    [Test]
    public void Lex_ReturnsDeleteToken_WhenProvidingDelete()
    {
        // Arrange
        const string deleteString = "delete";
        var lexer = new Lexer(deleteString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Delete, deleteString);
    }

    [Test]
    public void Lex_ReturnsDoToken_WhenProvidingDo()
    {
        // Arrange
        const string doString = "do";
        var lexer = new Lexer(doString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Do, doString);
    }

    [Test]
    public void Lex_ReturnsElseToken_WhenProvidingElse()
    {
        // Arrange
        const string elseString = "else";
        var lexer = new Lexer(elseString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Else, elseString);
    }

    [Test]
    public void Lex_ReturnsEnumToken_WhenProvidingEnum()
    {
        // Arrange
        const string enumString = "enum";
        var lexer = new Lexer(enumString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Enum, enumString);
    }

    [Test]
    public void Lex_ReturnsExportToken_WhenProvidingExport()
    {
        // Arrange
        const string exportString = "export";
        var lexer = new Lexer(exportString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Export, exportString);
    }

    [Test]
    public void Lex_ReturnsExtendsToken_WhenProvidingExtends()
    {
        // Arrange
        const string extendsString = "extends";
        var lexer = new Lexer(extendsString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Extends, extendsString);
    }

    [Test]
    public void Lex_ReturnsFalseToken_WhenProvidingFalse()
    {
        // Arrange
        const string falseString = "false";
        var lexer = new Lexer(falseString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.False, falseString);
    }

    [Test]
    public void Lex_ReturnsFinallyToken_WhenProvidingFinally()
    {
        // Arrange
        const string finallyString = "finally";
        var lexer = new Lexer(finallyString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Finally, finallyString);
    }

    [Test]
    public void Lex_ReturnsForToken_WhenProvidingFor()
    {
        // Arrange
        const string forString = "for";
        var lexer = new Lexer(forString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.For, forString);
    }

    [Test]
    public void Lex_ReturnsFunctionToken_WhenProvidingFunction()
    {
        // Arrange
        const string functionString = "function";
        var lexer = new Lexer(functionString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Function, functionString);
    }

    [Test]
    public void Lex_ReturnsIfToken_WhenProvidingIf()
    {
        // Arrange
        const string ifString = "if";
        var lexer = new Lexer(ifString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.If, ifString);
    }

    [Test]
    public void Lex_ReturnsImportToken_WhenProvidingImport()
    {
        // Arrange
        const string importString = "import";
        var lexer = new Lexer(importString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Import, importString);
    }

    [Test]
    public void Lex_ReturnsInToken_WhenProvidingIn()
    {
        // Arrange
        const string inString = "in";
        var lexer = new Lexer(inString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.In, inString);
    }

    [Test]
    public void Lex_ReturnsInstanceOfToken_WhenProvidingInstanceOf()
    {
        // Arrange
        const string instanceofString = "instanceof";
        var lexer = new Lexer(instanceofString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.InstanceOf, instanceofString);
    }

    [Test]
    public void Lex_ReturnsNewToken_WhenProvidingNew()
    {
        // Arrange
        const string newString = "new";
        var lexer = new Lexer(newString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.New, newString);
    }

    [Test]
    public void Lex_ReturnsNullToken_WhenProvidingNull()
    {
        // Arrange
        const string nullString = "null";
        var lexer = new Lexer(nullString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Null, nullString);
    }

    [Test]
    public void Lex_ReturnsReturnToken_WhenProvidingReturn()
    {
        // Arrange
        const string returnString = "return";
        var lexer = new Lexer(returnString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Return, returnString);
    }

    [Test]
    public void Lex_ReturnsSuperToken_WhenProvidingSuper()
    {
        // Arrange
        const string superString = "super";
        var lexer = new Lexer(superString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Super, superString);
    }

    [Test]
    public void Lex_ReturnsSwitchToken_WhenProvidingSwitch()
    {
        // Arrange
        const string switchString = "switch";
        var lexer = new Lexer(switchString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Switch, switchString);
    }

    [Test]
    public void Lex_ReturnsThisToken_WhenProvidingThis()
    {
        // Arrange
        const string thisString = "this";
        var lexer = new Lexer(thisString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.This, thisString);
    }

    [Test]
    public void Lex_ReturnsThrowToken_WhenProvidingThrow()
    {
        // Arrange
        const string throwString = "throw";
        var lexer = new Lexer(throwString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Throw, throwString);
    }

    [Test]
    public void Lex_ReturnsTrueToken_WhenProvidingTrue()
    {
        // Arrange
        const string trueString = "true";
        var lexer = new Lexer(trueString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.True, trueString);
    }

    [Test]
    public void Lex_ReturnsTryToken_WhenProvidingTry()
    {
        // Arrange
        const string tryString = "try";
        var lexer = new Lexer(tryString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Try, tryString);
    }

    [Test]
    public void Lex_ReturnsTypeOfToken_WhenProvidingTypeOf()
    {
        // Arrange
        const string typeOfString = "typeof";
        var lexer = new Lexer(typeOfString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.TypeOf, typeOfString);
    }

    [Test]
    public void Lex_ReturnsVarToken_WhenProvidingVar()
    {
        // Arrange
        const string varString = "var";
        var lexer = new Lexer(varString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Var, varString);
    }

    [Test]
    public void Lex_ReturnsVoidToken_WhenProvidingVoid()
    {
        // Arrange
        const string voidString = "void";
        var lexer = new Lexer(voidString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Void, voidString);
    }

    [Test]
    public void Lex_ReturnsWhileToken_WhenProvidingWhile()
    {
        // Arrange
        const string whileString = "while";
        var lexer = new Lexer(whileString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.While, whileString);
    }

    [Test]
    public void Lex_ReturnsWithToken_WhenProvidingWith()
    {
        // Arrange
        const string withString = "with";
        var lexer = new Lexer(withString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.With, withString);
    }

    [Test]
    public void Lex_ReturnsYieldToken_WhenProvidingYield()
    {
        // Arrange
        const string yieldString = "yield";
        var lexer = new Lexer(yieldString);

        // Act
        var tokens = lexer.Lex().ToList();

        // Assert
        Assert.That(tokens, Has.Count.EqualTo(1));
        AssertThatTokenIs(tokens[0], TokenType.Yield, yieldString);
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
