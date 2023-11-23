namespace JSS.Lib.AST.Values;

internal sealed class Null : Value
{
    public override bool IsNull() { return true; }

    override public bool Equals(object? obj)
    {
        if (obj is not Null) return false;
        return true;
    }

    override public int GetHashCode()
    {
        return nullGuid.GetHashCode();
    }

    // NOTE: This is a hack for making sure all nulls have the same hash code
    static private readonly Guid nullGuid = Guid.NewGuid();
}
