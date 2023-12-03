using Object = JSS.Lib.AST.Values.Object;

namespace JSS.Lib.Execution;

// 9.3 Realms, https://tc39.es/ecma262/#realm-record
internal sealed class Realm
{
    // 9.3.1 CreateRealm ( ), https://tc39.es/ecma262/#sec-createrealm
    public Realm()
    {
        // 1. Let realmRec be a new Realm Record.
        // FIXME: 2. Perform CreateIntrinsics(realmRec).

        // 3. Set realmRec.[[AgentSignifier]] to AgentSignifier(). 
        Agent = Agent.AgentSignifier();

        // FIXME: 4. Set realmRec.[[GlobalObject]] to undefined.
        GlobalObject = new(null);

        // FIXME: 5. Set realmRec.[[GlobalEnv]] to undefined.
        // FIXME: 6. Set realmRec.[[TemplateMap]] to a new empty List.

        // 7. Return realmRec.
    }

    // 9.4.6 GetGlobalObject ( ), https://tc39.es/ecma262/#sec-getglobalobject
    static public Object GetGlobalObject(VM vm)
    {
        // 1. Let currentRealm be the current Realm Record.
        var currentRealm = vm.Realm;

        // 2. Return currentRealm.[[GlobalObject]].
        return currentRealm.GlobalObject;
    }

    public Agent Agent { get; }
    public Object GlobalObject { get; }
    // FIXME: [[Intrinsics]]
    // FIXME: [[LoadedModules]]
}
