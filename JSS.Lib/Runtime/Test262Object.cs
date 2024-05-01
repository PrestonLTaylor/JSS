using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

internal sealed class Test262Object : Object
{
    public Test262Object(ObjectPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(VM vm)
    {
        var createRealmBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, createRealm);
        DataProperties.Add("createRealm", new Property(createRealmBuiltin, new(true, false, true)));
    }

    private Completion createRealm(VM _, Value? thisValue, List argumentList)
    {
        // createRealm - a function which creates a new ECMAScript Realm, defines this API on the new realm's global object,
        // and returns the $262 property of the new realm's global object
        var initializeCompletion = Realm.InitializeHostDefinedRealm(out VM newVm);
        if (initializeCompletion.IsAbruptCompletion()) return initializeCompletion;

        var test262DefiningCompletion = Realm.CreateTest262HostDefinedFunctions(newVm);
        if (test262DefiningCompletion.IsAbruptCompletion()) return test262DefiningCompletion;

        return newVm.Realm.GlobalObject.DataProperties["$262"].Value;
    }
}
