﻿namespace JSS.Lib.AST.Values;

internal sealed class Undefined : Value
{
    override public bool IsUndefined() { return true; }

    override public bool Equals(object? obj)
    {
        if (obj is not Undefined) return false;
        return true;
    }

    override public int GetHashCode()
    {
        return undefinedGuid.GetHashCode();
    }

    // NOTE: This is a hack for making sure all undefineds have the same hash code
    static private readonly Guid undefinedGuid = Guid.NewGuid();
}
