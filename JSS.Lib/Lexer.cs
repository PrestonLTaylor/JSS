namespace JSS.Lib;

internal sealed class Lexer
{
	// https://tc39.es/ecma262/#prod-ReservedWord
    static private readonly Dictionary<string, TokenType> reservedWordToType = new()
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

    // https://tc39.es/ecma262/#prod-Punctuator
    static private readonly Dictionary<string, TokenType> punctuatorToType = new()
	{
        // NOTE: Punctuators should be sorted by their length (maximal munch)
        { ">>>=", TokenType.UnsignedRightShiftAssignment },
        { "!==", TokenType.StrictNotEquals },
        { "&&=", TokenType.AndAssignment },
        { "**=", TokenType.ExponentiationAssignment },
        { "...", TokenType.Spread },
        { "/=", TokenType.DivisionAssignment },
        { "<<=", TokenType.LeftShiftAssignment },
        { "===", TokenType.StrictEqualsEquals },
        { ">>=", TokenType.RightShiftAssignment },
        { ">>>", TokenType.UnsignedRightShift },
        { "??=", TokenType.NullCoalescingAssignment },
        { "||=", TokenType.OrAssignment },
        { "!=", TokenType.NotEquals },
        { "%=", TokenType.ModuloAssignment },
        { "&&", TokenType.And },
        { "&=", TokenType.BitwiseAndAssignment },
        { "**", TokenType.Exponentiation },
        { "*=", TokenType.MultiplyAssignment },
        { "++", TokenType.Increment },
        { "+=", TokenType.PlusAssignment },
        { "-=", TokenType.MinusAssignment },
        { "<<", TokenType.LeftShift },
        { "<=", TokenType.LessThanEqual },
        { "==", TokenType.EqualEquals },
        { "=>", TokenType.ArrowFunction },
        { ">=", TokenType.GreaterThanEqual },
        { ">>", TokenType.RightShift },
        { "?.", TokenType.OptionalChaining },
        { "??", TokenType.NullCoalescing },
        { "^=", TokenType.BitwiseXorAssignment },
        { "{", TokenType.OpenBrace },
        { "|=", TokenType.BitwiseOrAssignment },
        { "||", TokenType.Or },
        { "!", TokenType.Not },
        { "%", TokenType.Modulo },
        { "&", TokenType.BitwiseAnd },
        { "(", TokenType.OpenParen },
        { ")", TokenType.ClosedParen },
        { "*", TokenType.Multiply },
        { "+", TokenType.Plus },
        { ",", TokenType.Comma },
        { "-", TokenType.Minus },
        { ".", TokenType.Dot },
        { "/", TokenType.Division },
        { ":", TokenType.Colon },
        { ";", TokenType.SemiColon },
        { "<", TokenType.LessThan },
        { "=", TokenType.Assignment },
        { ">", TokenType.GreaterThan },
        { "?", TokenType.Ternary },
        { "[", TokenType.OpenSquare },
        { "]", TokenType.ClosedSquare },
        { "^", TokenType.BitwiseXor },
        { "|", TokenType.BitwiseOr },
        { "}", TokenType.ClosedBrace },
        { "~", TokenType.BitwiseNot },
    };

    private readonly Consumer _consumer;

	public Lexer(string toLex)
	{
		_consumer = new Consumer(toLex);
	}

    // FIXME: Fix "maximal munch" temporal issues
	public IEnumerable<Token> Lex()
	{
		while (_consumer.CanConsume())
		{
			if (TryLexReservedWord(out Token? reservedWordToken))
			{
				yield return reservedWordToken!.Value;
			}
			else if (IsWhiteSpace())
			{
				IgnoreWhiteSpace();
			}
			else if (IsLineTerminator())
			{
				yield return LexLineTerminator();
			}
			else if (IsSingleLineComment())
			{
				IgnoreSingleLineComment();
			}
			else if (IsMultiLineComment())
			{
				IgnoreMultiLineComment();
			}
			else if (IsHashBangComment())
			{
				IgnoreHashBangComment();
			}
			else if (IsPrivateIdentifier())
			{
				yield return LexPrivateIdentifier();
			}
			else if (IsIdentifierStart())
			{
				yield return LexIdentifier();
			}
            else if (IsDecimalLiteral())
            {
                yield return LexNumericLiteral();
            }
            else if (IsStringLiteral())
            {
                yield return LexStringLiteral();
            }
            // NOTE: Lexxing of punctuators is at the end because of symbols like "/*" being lexed as a punctuator
            else if (TryLexPunctuator(out Token? punctuatorToken))
            {
                yield return punctuatorToken!.Value;
            }
            else
			{
				throw new NotImplementedException();
			}
		}

		yield break;
	}

    // 12.2 White Space, https://tc39.es/ecma262/#sec-white-space
    private bool IsWhiteSpace()
	{
		const string whiteSpaceCodePoints = "\u0009\u000B\u000C\uFEFF\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000";

		var codePoint = _consumer.Peek();
		return whiteSpaceCodePoints.Contains(codePoint);
    }

	private void IgnoreWhiteSpace()
	{
		_consumer.ConsumeWhile((_) => IsWhiteSpace());
	}

    // 12.3 Line Terminators, https://tc39.es/ecma262/#sec-line-terminators
    private bool IsLineTerminator()
	{
        const string lineTerminatorCodePoints = "\u000A\u000D\u2028\u2029";

		var codePoint = _consumer.Peek();
        return lineTerminatorCodePoints.Contains(codePoint);
    }

	private Token LexLineTerminator()
	{
		return new Token { type = TokenType.LineTerminator, data = _consumer.Consume().ToString() };
	}

    // 12.4 Comments, https://tc39.es/ecma262/#sec-comments
	private bool IsSingleLineComment()
	{
		return _consumer.Matches("//");
	}

    private void IgnoreSingleLineComment()
	{
		_consumer.ConsumeWhile((_) => !IsLineTerminator());
	}

	private bool IsMultiLineComment()
	{
		return _consumer.Matches("/*");
	}

    // FIXME: Comments behave like white space and are discarded except that, if a MultiLineComment contains a line terminator code point,
    // then the entire comment is considered to be a LineTerminator for purposes of parsing by the syntactic grammar
    private void IgnoreMultiLineComment()
	{
		_consumer.ConsumeWhile((_) => !_consumer.Matches("*/"));

		// FIXME: If the consumer is at the end of the string then the multi-line comment had no ending "*/"
		// then we need to produce a SyntaxError
		_consumer.TryConsumeString("*/");
	}

    // 12.5 Hashbang Comments, https://tc39.es/ecma262/#sec-hashbang
    private bool IsHashBangComment()
	{
		return _consumer.Matches("#!");
	}

	private void IgnoreHashBangComment()
	{
		_consumer.ConsumeWhile((_) => !IsLineTerminator());
	}

    // 12.7.1 Identifier Names, https://tc39.es/ecma262/#sec-identifier-names
    // https://tc39.es/ecma262/#prod-PrivateIdentifier
    private bool IsPrivateIdentifier()
	{
		return _consumer.Peek() == '#';
	}

	private Token LexPrivateIdentifier()
	{
		// Consumes the # so we can lex the identifier
		_consumer.Consume();
		var identifierToken = LexIdentifier();
		identifierToken.type = TokenType.PrivateIdentifier;
		identifierToken.data = '#' + identifierToken.data;
		return identifierToken;
	}

    // https://tc39.es/ecma262/#prod-IdentifierStart
    private bool IsIdentifierStart(int offset = 0)
	{
		if (!_consumer.CanConsume(offset)) return false;

		// FIXME: We want to lex unicode escape sequences
		// FIXME: Match the whole set of ID_Start code points: https://unicode.org/reports/tr31/#D1 
		var codePoint = _consumer.Peek(offset);
		return char.IsLetter(codePoint) || codePoint == '$' || codePoint == '_';
	}

    // https://tc39.es/ecma262/#prod-IdentifierPart
    private bool IsIdentifierPart(int offset = 0)
	{
		if (!_consumer.CanConsume(offset)) return false;
		if (IsIdentifierStart(offset)) return true;

		// FIXME: We want to lex unicode escape sequences
		// FIXME: Match the whole set of ID_Continue code points: https://unicode.org/reports/tr31/#D1 
		var codePoint = _consumer.Peek(offset);
		return char.IsLetterOrDigit(codePoint);
    }

	private Token LexIdentifier()
	{
		var consumedIdentifier = _consumer.ConsumeWhile((_) => IsIdentifierPart());
		return new Token { type = TokenType.Identifier, data = consumedIdentifier };
	}

    // 12.7.2 Keywords and Reserved Words, https://tc39.es/ecma262/#sec-keywords-and-reserved-words
    // https://tc39.es/ecma262/#prod-ReservedWord
    private bool TryLexReservedWord(out Token? token)
	{
		foreach (var kv in reservedWordToType)
		{
			var reservedWord = kv.Key;
			// We watch to match the reserved word but only match if there isn't an identifier part at the end
			if (_consumer.Matches(reservedWord) && !IsIdentifierPart(reservedWord.Length))
			{
				_consumer.TryConsumeString(reservedWord);

				var tokenType = kv.Value;
				token = new Token { type = tokenType, data = reservedWord };
				return true;
			}
		}

		token = null;
		return false;
	}

    // 12.8 Punctuators, https://tc39.es/ecma262/#sec-punctuators
	private bool TryLexPunctuator(out Token? token)
	{
        foreach (var kv in punctuatorToType)
        {
            var punctuator = kv.Key;
            if (_consumer.Matches(punctuator))
            {
                _consumer.TryConsumeString(punctuator);

                var tokenType = kv.Value;
                token = new Token { type = tokenType, data = punctuator };
                return true;
            }
        }

        token = null;
        return false;
    }

    // 12.9.3 Numeric Literals, https://tc39.es/ecma262/#sec-literals-numeric-literals
    // FIXME: Implement support for fully lexxing https://tc39.es/ecma262/#prod-NumericLiteral
    // https://tc39.es/ecma262/#prod-DecimalIntegerLiteral
    private bool IsDecimalLiteral()
    {
        var codePoint = _consumer.Peek();
        return char.IsNumber(codePoint);
    }

    private Token LexNumericLiteral()
    {
        // FIXME: The SourceCharacter immediately following a NumericLiteral must not be an IdentifierStart or DecimalDigit.
        var consumedLiteral = _consumer.ConsumeWhile((_) => IsDecimalLiteral());
        return new Token { type = TokenType.Number, data = consumedLiteral };
    }

    // 12.9.4 String Literals, https://tc39.es/ecma262/#sec-literals-string-literals
    private bool IsStringLiteral()
    {
        return _consumer.Peek() == '\'' || _consumer.Peek() == '"';
    }

    // FIXME: Implement support for fully lexxing https://tc39.es/ecma262/#prod-StringLiteral (e.g. escape sequences)
    private Token LexStringLiteral()
    {
        var quoteCodePoint = _consumer.Consume();

        var consumedString = _consumer.ConsumeWhile((codePoint) => codePoint != quoteCodePoint);

        // FIXME: Error if no closing quote is given
        _consumer.TryConsumeString(quoteCodePoint.ToString());

        return new Token { type = TokenType.String, data = quoteCodePoint + consumedString + quoteCodePoint };
    }

    // FIXME: 12.9.5 Regular Expression Literals, https://tc39.es/ecma262/#sec-literals-regular-expression-literals
    // FIXME: 12.9.6 Template Literal Lexical Components, https://tc39.es/ecma262/#sec-template-literal-lexical-components
    // FIXME: 12.10 Automatic Semicolon Insertion, https://tc39.es/ecma262/#sec-automatic-semicolon-insertion
}