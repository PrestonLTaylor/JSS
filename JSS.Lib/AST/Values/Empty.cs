namespace JSS.Lib.AST.Values;

internal sealed class Empty : Value
{
    private Empty() { }

    override public bool IsEmpty() { return true; }
    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of EMPTY"); }

    static public Empty The
    { 
        get
        {
            return _empty;
        }
    }
    static readonly private Empty _empty = new();
}
