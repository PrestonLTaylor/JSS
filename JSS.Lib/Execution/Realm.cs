﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JSS.Lib.AST.Values;
using JSS.Lib.Runtime;

namespace JSS.Lib.Execution;

// 9.3 Realms, https://tc39.es/ecma262/#realm-record
public sealed class Realm
{
    // 9.3.1 CreateRealm ( ), https://tc39.es/ecma262/#sec-createrealm
    internal Realm()
    {
        // 1. Let realmRec be a new Realm Record.

        // 2. Perform CreateIntrinsics(realmRec).
        CreateIntrinsics();

        // 3. Set realmRec.[[AgentSignifier]] to AgentSignifier(). 
        Agent = Agent.AgentSignifier();

        // 4. Set realmRec.[[GlobalObject]] to undefined.
        GlobalObject = Undefined.The;

        // FIXME: 5. Set realmRec.[[GlobalEnv]] to undefined.
        GlobalEnv = null;

        // FIXME: 6. Set realmRec.[[TemplateMap]] to a new empty List.

        // 7. Return realmRec.
    }

    // 9.3.2 CreateIntrinsics ( realmRec ), https://tc39.es/ecma262/#sec-createintrinsics
    [MemberNotNull(nameof(ObjectPrototype), nameof(FunctionPrototype), nameof(ObjectConstructor))]
    private void CreateIntrinsics()
    {
        // 1. Set realmRec.[[Intrinsics]] to a new Record.

        // 2. Set fields of realmRec.[[Intrinsics]] with the values listed in Table 6. The field names are the names listed in column one of the table.
        // The value of each field is a new object value fully and recursively populated with property values as defined by the specification of each object in
        // clauses 19 through 28. All object property values are newly created object values.
        // All values that are built-in function objects are created by performing CreateBuiltinFunction(steps, length, name, slots, realmRec, prototype)
        // where steps is the definition of that function provided by this specification, name is the initial value of the function's "name" property,
        // length is the initial value of the function's "length" property, slots is a list of the names, if any, of the function's specified internal slots,
        // and prototype is the specified value of the function's [[Prototype]] internal slot. The creation of the intrinsics and their properties must be ordered
        // to avoid any dependencies upon objects that have not yet been created.

        // NOTE: We first create the required objects and then initialize their properties, so we can handle circular dependencies
        ObjectPrototype = new();
        FunctionPrototype = new(ObjectPrototype);
        ObjectConstructor = new(FunctionPrototype);

        ObjectPrototype.Initialize(this);
        ObjectConstructor.Initialize();

        // FIMXE: 3. Perform AddRestrictedFunctionProperties(realmRec.[[Intrinsics]].[[%Function.prototype%]], realmRec).

        // 4. Return UNUSED.
    }

    // 9.3.3 SetRealmGlobalObject ( realmRec, globalObj, thisValue ), https://tc39.es/ecma262/#sec-setrealmglobalobject
    internal void SetRealmGlobalObject(Object globalObj, Object thisValue)
    {
        // 1. If globalObj is undefined, then
        if (globalObj.IsUndefined())
        {
            // a. Let intrinsics be realmRec.[[Intrinsics]].
            // b. Set globalObj to OrdinaryObjectCreate(intrinsics.[[% Object.prototype %]]).
            globalObj = new Object(ObjectPrototype);
        }

        // 2. Assert: globalObj is an Object.
        Debug.Assert(globalObj.IsObject());

        // 3. If thisValue is undefined, set thisValue to globalObj.
        if (thisValue.IsUndefined())
        {
            thisValue = globalObj;
        }

        // 4. Set realmRec.[[GlobalObject]] to globalObj.
        GlobalObject = globalObj;

        // 5. Let newGlobalEnv be NewGlobalEnvironment(globalObj, thisValue).
        var newGlobalEnv = new GlobalEnvironment(globalObj, thisValue);

        // 6. Set realmRec.[[GlobalEnv]] to newGlobalEnv.
        GlobalEnv = newGlobalEnv;

        // 7. Return UNUSED.
    }

    // 9.3.4 SetDefaultGlobalBindings ( realmRec ), https://tc39.es/ecma262/#sec-setdefaultglobalbindings
    private Completion SetDefaultGlobalBindings()
    {
        // 1. Let global be realmRec.[[GlobalObject]].

        // 2. For each property of the Global Object specified in clause 19, do
        var globalProperties = CreateGlobalProperties();
        foreach (var property in globalProperties)
        {
            // a. Let name be the String value of the property name.
            var name = property.Key;

            // b. Let desc be the fully populated data Property Descriptor for the property, containing the specified attributes for the property.
            // FIXME: For properties listed in 19.2, 19.3, or 19.4 the value of the [[Value]] attribute is the corresponding intrinsic object from realmRec.
            var desc = property.Value;

            // c. Perform ? DefinePropertyOrThrow(global, name, desc).
            var defineResult = Object.DefinePropertyOrThrow(GlobalObject, name, desc);
            if (defineResult.IsAbruptCompletion()) return defineResult;
        }

        // 3. Return global.
        return GlobalObject;
    }

    private Dictionary<string, Property> CreateGlobalProperties()
    {
        Dictionary<string, Property> globalProperties = new();

        // 19.1 Value Properties of the Global Object
        // 19.1.4 undefined
        globalProperties.Add("undefined", new Property(Undefined.The, new(false, false, false)));

        // 19.3 Constructor Properties of the Global Object
        // 20.1.1 The Object Constructor, https://tc39.es/ecma262/#sec-object-constructor
        globalProperties.Add("Object", new Property(ObjectConstructor, new(true, false, true)));

        return globalProperties;
    }

    // 9.4.6 GetGlobalObject ( ), https://tc39.es/ecma262/#sec-getglobalobject
    static internal Object GetGlobalObject(VM vm)
    {
        // 1. Let currentRealm be the current Realm Record.
        var currentRealm = vm.Realm;

        // 2. Return currentRealm.[[GlobalObject]].
        return currentRealm.GlobalObject;
    }

    // 9.6 InitializeHostDefinedRealm ( ), https://tc39.es/ecma262/#sec-initializehostdefinedrealm
    static public Completion InitializeHostDefinedRealm(out VM vm)
    {
        // 1. Let realm be CreateRealm().
        var realm = new Realm();
        vm = new VM(realm);

        // 2. Let newContext be a new execution context.
        // FIXME: 3. Set the Function of newContext to null.
        // 4. Set the Realm of newContext to realm.
        // FIXME: 5. Set the ScriptOrModule of newContext to null.
        var newContext = new ExecutionContext(realm);

        // 6. Push newContext onto the execution context stack; newContext is now the running execution context.
        vm.PushExecutionContext(newContext);

        // 7. If the host requires use of an exotic object to serve as realm's global object, let global be such an object created in a host-defined manner.
        // Otherwise, let global be undefined, indicating that an ordinary object should be created as the global object.
        var global = Undefined.The;

        // 8. If the host requires that the this binding in realm's global scope return an object other than the global object,
        // let thisValue be such an object created in a host-defined manner.
        // Otherwise, let thisValue be undefined, indicating that realm's global this binding should be the global object.
        var thisValue = Undefined.The;

        // 9. Perform SetRealmGlobalObject(realm, global, thisValue).
        realm.SetRealmGlobalObject(global, thisValue);

        // 10. Let globalObj be ? SetDefaultGlobalBindings(realm).
        var globalObj = realm.SetDefaultGlobalBindings();
        if (globalObj.IsAbruptCompletion()) return globalObj;

        // FIXME: 11. Create any host-defined global object properties on globalObj.

        // 12. Return UNUSED.
        return Empty.The;
    }

    internal Agent Agent { get; }
    internal Object GlobalObject { get; private set; }
    internal GlobalEnvironment? GlobalEnv { get; private set; }
    // FIXME: [[LoadedModules]]

    // NOTE: We don't use a real record as the spec says we should, however, there should be no visible differences in the runtime.
    internal ObjectPrototype ObjectPrototype { get; private set; }
    internal ObjectConstructor ObjectConstructor { get; private set; }
    internal FunctionPrototype FunctionPrototype { get; private set; }
}
