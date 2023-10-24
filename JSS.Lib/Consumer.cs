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

    public char Peek()
    {
        return _toConsume[_index];
    }
}
