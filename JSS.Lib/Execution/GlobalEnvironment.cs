namespace JSS.Lib.Execution;

// 9.1.1.4 Global Environment Records, https://tc39.es/ecma262/#sec-global-environment-records
internal sealed class GlobalEnvironment : Environment
{
    public GlobalEnvironment() : base(null)
    {
        DeclarativeEnvironment = new(this);
        VarNames = new();
    }

    // 9.1.1.4.1 HasBinding( N ), https://tc39.es/ecma262/#sec-global-environment-records-hasbinding-n
    override public bool HasBinding(string N)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, return true.
        if (DeclarativeEnvironment.HasBinding(N)) { return true; }

        // FIXME: 3. Let ObjRec be envRec.[[ObjectRecord]].
        // FIXME: 4. Return ? ObjRec.HasBinding(N).
        return false;
    }

    // FIXME: [[ObjectRecord]]
    // FIXME: [[GlobalThisValue]]
    public DeclarativeEnvironment DeclarativeEnvironment { get; }
    public List<string> VarNames { get; }
}
