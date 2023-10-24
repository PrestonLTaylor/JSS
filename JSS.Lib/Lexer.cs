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
			else
			{
				throw new NotImplementedException();
			}
		}

		yield break;
	}

    // 12.2 White Space, https://tc39.es/ecma262/#sec-white-space
    private bool IsWhiteSpace(char codePoint)
	{
		const string whiteSpaceCodePoints = "\u0009\u000B\u000C\uFEFF\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u3000";
		return whiteSpaceCodePoints.Contains(codePoint);
    }

    // 12.3 Line Terminators, https://tc39.es/ecma262/#sec-line-terminators
    private bool IsLineTerminator(char codePoint)
	{
        const string lineTerminatorCodePoints = "\u000A\u000D\u2028\u2029";
        return lineTerminatorCodePoints.Contains(codePoint);
    }
}