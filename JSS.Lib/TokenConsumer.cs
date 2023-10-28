namespace JSS.Lib;

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
        return _toConsume[_index++];
    }

    public Token Peek(int offset = 0)
    {
        return _toConsume[_index + offset];
    }

    public bool IsTokenOfType(TokenType type)
    {
        return Peek().type == type;
    }

    // NOTE: This function has an invariant that the next token MUST be of the same type, otherwise it is a parser bug
    // FIXME: Introduce proper reporting of parser bugs
    public Token ConsumeTokenOfType(TokenType type)
    {
        if (!IsTokenOfType(type)) throw new InvalidOperationException($"Parser Bug: Tried to consume a token of type {type} but got {Peek().type}");
        return Consume();
    }
}
