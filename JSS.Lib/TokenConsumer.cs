﻿namespace JSS.Lib;

// FIXME: Make a generic Consumer, so we don't have TokenConsumer and Consumer (Currently TokenConsumer is not Unit Tested!)
internal sealed class TokenConsumer
{
    private readonly List<Token> _toConsume;
    private int _index = 0;

    public TokenConsumer(List<Token> tokens)
    {
        _toConsume = tokens;
    }

    public bool CanConsume(int offset = 0) => _index + offset < _toConsume.Count;

    public Token Consume()
    {
        IgnoreLineTerminators();
        return _toConsume[_index++];
    }

    public Token Peek(int offset = 0)
    {
        IgnoreLineTerminators();
        return _toConsume[_index + offset];
    }

    public bool IsTokenOfType(TokenType type)
    {
        IgnoreLineTerminators();
        return CanConsume() && Peek().type == type;
    }

    public Token ConsumeTokenOfType(TokenType type)
    {
        IgnoreLineTerminators();
        if (!CanConsume()) ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedEOF);
        if (!IsTokenOfType(type)) ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedToken, Peek().data);
        return Consume();
    }

    public bool IsLineTerminator()
    {
        if (!CanConsume()) return false;
        return _toConsume[_index].type == TokenType.LineTerminator;
    }

    public void IgnoreLineTerminators()
    {
        while (CanConsume() && _toConsume[_index].type == TokenType.LineTerminator) ++_index;
    }
}
