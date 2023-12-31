﻿namespace JSS.Lib;

internal sealed class Consumer
{
    private readonly string _toConsume;
    private int _index = 0;

    public Consumer(string toConsume)
    {
        _toConsume = toConsume;
    }

    public bool CanConsume(int offset = 0) => _index + offset <_toConsume.Length;

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

    public bool TryConsumeString(string toConsume)
    {
        if (!Matches(toConsume)) return false;

        _index += toConsume.Length;
        return true;
    }

    public char Peek(int offset = 0)
    {
        return _toConsume[_index + offset];
    }

    public bool Matches(string toMatch)
    {
        if (toMatch.Length > Remaining()) return false;
        if (!CanConsume()) return string.IsNullOrEmpty(toMatch);

        string substring = _toConsume.Substring(_index, toMatch.Length);
        return substring == toMatch;
    }

    private int Remaining()
    {
        return _toConsume.Length - _index;
    }
}
