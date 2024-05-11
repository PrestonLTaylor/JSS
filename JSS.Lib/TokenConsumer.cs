namespace JSS.Lib;

// FIXME: Make a generic Consumer, so we don't have TokenConsumer and Consumer (Currently TokenConsumer is not Unit Tested!)
internal sealed class TokenConsumer
{
    private readonly List<Token> _toConsume;
    public int Index { get; private set; } = 0;

    public TokenConsumer(List<Token> tokens)
    {
        _toConsume = tokens;
    }

    public bool CanConsume(int offset = 0) => Index + offset < _toConsume.Count;

    public Token Consume()
    {
        IgnoreLineTerminators();
        return _toConsume[Index++];
    }

    public Token Peek(int offset = 0)
    {
        IgnoreLineTerminators();
        return _toConsume[Index + offset];
    }

    public bool IsTokenOfType(TokenType type, int offset = 0)
    {
        IgnoreLineTerminators();
        return CanConsume(offset) && Peek(offset).type == type;
    }

    public Token ConsumeTokenOfType(TokenType type)
    {
        IgnoreLineTerminators();
        if (!CanConsume()) ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedEOF);
        if (!IsTokenOfType(type)) ErrorHelper.ThrowSyntaxError(ErrorType.UnexpectedToken, Peek().data);
        return Consume();
    }

    public bool IsLineTerminator(int offset = 0)
    {
        if (!CanConsume()) return false;
        return _toConsume[Index + offset].type == TokenType.LineTerminator;
    }

    public void IgnoreLineTerminators()
    {
        while (CanConsume() && _toConsume[Index].type == TokenType.LineTerminator) ++Index;
    }

    public void Rewind(int index = 0)
    {
        Index = index;
    }
}
