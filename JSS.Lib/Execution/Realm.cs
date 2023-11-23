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
        // FIXME: 5. Set realmRec.[[GlobalEnv]] to undefined.
        // FIXME: 6. Set realmRec.[[TemplateMap]] to a new empty List.

        // 7. Return realmRec.
    }

    public Agent Agent { get; }
    // FIXME: [[Intrinsics]]
    // FIXME: [[LoadedModules]]
}
