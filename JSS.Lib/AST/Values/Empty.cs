namespace JSS.Lib.AST.Values;

internal class Empty : Value
{
    override public bool IsEmpty() { return true; }
    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of EMPTY"); }
}
