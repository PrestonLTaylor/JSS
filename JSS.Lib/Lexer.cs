namespace JSS.Lib;

internal sealed class Lexer
{
	private readonly Consumer consumer;

	public Lexer(string toLex)
	{
		consumer = new Consumer(toLex);
	}

	public IEnumerable<Token> Lex()
	{
		while (consumer.CanConsume())
		{
			// FIXME: Clean up after implemented
			var current = consumer.Peek();
			if (IsWhiteSpace(current))
			{
				consumer.Consume();
			}
			else if (IsLineTerminator(current))
			{
				yield return new Token { type = TokenType.LineTerminator, data = consumer.Consume().ToString() };	
			}
			else if (IsComment(current))
			{
				IgnoreComment();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		yield break;
	}

    // 12.2 White Space, https://tc39.es/ecma262/#sec-white-space
    static private bool IsWhiteSpace(char codePoint)
	{
		const string whiteSpaceCodePoints = "\u0009\u000B\u000C\uFEFF\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000";
		return whiteSpaceCodePoints.Contains(codePoint);
    }

    // 12.3 Line Terminators, https://tc39.es/ecma262/#sec-line-terminators
    static private bool IsLineTerminator(char codePoint)
	{
        const string lineTerminatorCodePoints = "\u000A\u000D\u2028\u2029";
        return lineTerminatorCodePoints.Contains(codePoint);
    }

    // 12.4 Comments, https://tc39.es/ecma262/#sec-comments
	private bool IsComment(char firstCodePoint)
	{
		var secondCodePoint = consumer.Peek(1);
		return firstCodePoint == '/' && secondCodePoint == '/';
	}

    // FIXME: Comments behave like white space and are discarded except that, if a MultiLineComment contains a line terminator code point,
	// then the entire comment is considered to be a LineTerminator for purposes of parsing by the syntactic grammar
    private void IgnoreComment()
	{
		consumer.ConsumeWhile((codePoint) => !IsLineTerminator(codePoint));
	}
}