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

        var evalScriptBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, evalScript);
        DataProperties.Add("evalScript", new Property(evalScriptBuiltin, new(true, false, true)));

        var gcBuiltin = BuiltinFunction.CreateBuiltinFunction(vm, gc);
        DataProperties.Add("gc", new Property(gcBuiltin, new(true, false, true)));

        // global, a reference to the global object on which $262 was initially defined
        DataProperties.Add("global", new Property(vm.Realm.GlobalObject, new(true, false, true)));
    }

    private Completion createRealm(VM _, Value thisValue, List argumentList)
    {
        // createRealm - a function which creates a new ECMAScript Realm, defines this API on the new realm's global object,
        // and returns the $262 property of the new realm's global object
        var initializeCompletion = Realm.InitializeHostDefinedRealm(out VM newVm);
        if (initializeCompletion.IsAbruptCompletion()) return initializeCompletion;

        var test262DefiningCompletion = Realm.CreateTest262HostDefinedFunctions(newVm);
        if (test262DefiningCompletion.IsAbruptCompletion()) return test262DefiningCompletion;

        return newVm.Realm.GlobalObject.DataProperties["$262"].Value;
    }

    // evalScript - a function which accepts a string value as its first argument and executes it as an ECMAScript script
    private Completion evalScript(VM vm, Value thisValue, List argumentList)
    {
        // 1. Let hostDefined be any host-defined values for the provide sourceText (obtained in an implementation dependent manner)
        var sourceText = argumentList[0].AsString();

        // 2. Let realm be the current Realm Record.
        // 3. Let s be ParseScript(sourceText, realm, hostDefined).
        Script? s;
        try
        {
            var parser = new Parser(sourceText);
            s = parser.Parse(vm);
        }
        catch (SyntaxErrorException ex)
        {
            // 4. If s is a List of errors, then
            // a. Let error be the first element of s.
            var error = ex.Message;

            // b. Return Completion { [[Type]]: throw, [[Value]]: error, [[Target]]: empty }.
            return Completion.ThrowCompletion(error);
        }

        // 5. Let status be ScriptEvaluation(s).
        // 6. Return Completion(status).
        return s!.ScriptEvaluation();
    }

    private Completion gc(VM vm, Value thisValue, List argumentList)
    {
        // gc, a function that wraps the host's garbage collection invocation mechanism
        GC.Collect();
        return Undefined.The;
    }
}
