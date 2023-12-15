namespace JSS.Lib.AST.Values;

internal sealed class Null : Value
{
    private Null() { }

    public override bool IsNull() { return true; }
    override public ValueType Type() { return ValueType.Null; }

    override public bool Equals(object? obj)
    {
        if (obj is not Null) return false;
        return true;
    }

    override public int GetHashCode()
    {
        return nullGuid.GetHashCode();
    }

    static public Null The
    {
        get
        {
            return _null;
        }
    }
    static readonly private Null _null = new();

    // NOTE: This is a hack for making sure all nulls have the same hash code
    static private readonly Guid nullGuid = Guid.NewGuid();
}
