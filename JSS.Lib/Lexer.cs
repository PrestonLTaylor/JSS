﻿namespace JSS.Lib;

internal sealed class Lexer
{
	private readonly Consumer _consumer;

	public Lexer(string toLex)
	{
		_consumer = new Consumer(toLex);
	}

	public IEnumerable<Token> Lex()
	{
		while (_consumer.CanConsume())
		{
			if (IsWhiteSpace())
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
}