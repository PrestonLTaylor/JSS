using Object = JSS.Lib.AST.Values.Object;

namespace JSS.Lib.Execution;

// 9.1.1.4 Global Environment Records, https://tc39.es/ecma262/#sec-global-environment-records
internal sealed class GlobalEnvironment : Environment
{
    // 9.1.2.5 NewGlobalEnvironment ( G, thisValue ), https://tc39.es/ecma262/#sec-newglobalenvironment
    public GlobalEnvironment(Object G, Object thisValue)
    {
        // 1. Let objRec be NewObjectEnvironment(G, false, null).
        var objRec = new ObjectEnvironment(G, false, null);

        // 2. Let dclRec be NewDeclarativeEnvironment(null).
        var dclRec = new DeclarativeEnvironment(null);

        // 3. Let env be a new Global Environment Record.

        // 4. Set env.[[ObjectRecord]] to objRec.
        ObjectRecord = objRec;

        // 5. Set env.[[GlobalThisValue]] to thisValue.
        GlobalThisValue = thisValue;

        // 6. Set env.[[DeclarativeRecord]] to dclRec.
        DeclarativeRecord = dclRec;

        // 7. Set env.[[VarNames]] to a new empty List.
        VarNames = new();

        // 8. Set env.[[OuterEnv]] to null.
        OuterEnv = null;

        // 9. Return env.
    }

    // 9.1.1.4.1 HasBinding( N ), https://tc39.es/ecma262/#sec-global-environment-records-hasbinding-n
    override public bool HasBinding(string N)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, return true.
        if (DeclarativeRecord.HasBinding(N)) { return true; }

        // FIXME: 3. Let ObjRec be envRec.[[ObjectRecord]].
        // FIXME: 4. Return ? ObjRec.HasBinding(N).
        return false;
    }

    public ObjectEnvironment ObjectRecord { get; }
    public Object GlobalThisValue { get; }
    public DeclarativeEnvironment DeclarativeRecord { get; }
    public List<string> VarNames { get; }
}
