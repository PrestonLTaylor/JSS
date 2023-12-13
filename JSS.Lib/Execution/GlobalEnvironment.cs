using JSS.Lib.AST.Values;
using System.Diagnostics;
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

        // 3. Let ObjRec be envRec.[[ObjectRecord]].
        // 4. Return ? ObjRec.HasBinding(N).
        return ObjectRecord.HasBinding(N);
    }

    // 9.1.1.4.4 InitializeBinding ( N, V ), https://tc39.es/ecma262/#sec-global-environment-records-initializebinding-n-v
    override public Completion InitializeBinding(VM vm, string N, Value V)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ! DclRec.InitializeBinding(N, V).
            return DeclarativeRecord.InitializeBinding(vm, N, V);
        }

        // 3. Assert: If the binding exists, it must be in the Object Environment Record.
        Debug.Assert(ObjectRecord.HasBinding(N));

        // 4. Let ObjRec be envRec.[[ObjectRecord]].
        // 5. Return ? ObjRec.InitializeBinding(N, V).
        return ObjectRecord.InitializeBinding(vm, N, V);
    }

    // 9.1.1.4.5 SetMutableBinding ( N, V, S ), https://tc39.es/ecma262/#sec-global-environment-records-setmutablebinding-n-v-s
    override public Completion SetMutableBinding(VM vm, string N, Value V, bool S)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ? DclRec.SetMutableBinding(N, V, S).
            return DeclarativeRecord.SetMutableBinding(vm, N, V, S);
        }

        // 3. Let ObjRec be envRec.[[ObjectRecord]].
        // 4. Return ? ObjRec.SetMutableBinding(N, V, S).
        return ObjectRecord.SetMutableBinding(vm, N, V, S);
    }

    // 9.1.1.4.6 GetBindingValue ( N, S ), https://tc39.es/ecma262/#sec-global-environment-records-getbindingvalue-n-s
    override public Completion GetBindingValue(string N, bool S)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ? DclRec.GetBindingValue(N, S).
            return DeclarativeRecord.GetBindingValue(N, S);
        }

        // 3. Let ObjRec be envRec.[[ObjectRecord]].
        // 4. Return ? ObjRec.GetBindingValue(N, S).
        return ObjectRecord.GetBindingValue(N, S);
    }

    // 9.1.1.4.12 HasVarDeclaration ( N ), https://tc39.es/ecma262/#sec-hasvardeclaration
    public bool HasVarDeclaration(string N)
    {
        // 1. Let varDeclaredNames be envRec.[[VarNames]].
        // 2. If varDeclaredNames contains N, return true.
        // 3. Return false.
        return VarNames.Contains(N);
    }

    public ObjectEnvironment ObjectRecord { get; }
    public Object GlobalThisValue { get; }
    public DeclarativeEnvironment DeclarativeRecord { get; }
    public List<string> VarNames { get; }
}
