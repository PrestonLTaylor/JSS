namespace JSS.Lib;

internal sealed class Consumer
{
    private readonly string _toConsume;
    private int _index = 0;

    public Consumer(string toConsume)
    {
        _toConsume = toConsume;
    }

    public bool CanConsume() => _index < _toConsume.Length;

    public char Consume()
    {
        return _toConsume[_index++];
    }

    // FIXME: Return a Span instead of a string
    public string ConsumeWhile(Predicate<char> predicate)
    {
        string consumed = "";
        while (CanConsume() && predicate(Peek()))
        {
            consumed += Consume();
        }

        return consumed;
    }

    public char Peek()
    {
        return _toConsume[_index];
    }

    public bool Matches(string toMatch)
    {
        if (!CanConsume()) return string.IsNullOrEmpty(toMatch);

        string substring = _toConsume.Substring(_index, toMatch.Length);
        return substring == toMatch;
    }
}
