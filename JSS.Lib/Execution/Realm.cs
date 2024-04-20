using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JSS.Lib.AST.Values;
using JSS.Lib.Runtime;

namespace JSS.Lib.Execution;

// 9.3 Realms, https://tc39.es/ecma262/#realm-record
public sealed class Realm
{
    // 9.3.1 CreateRealm ( ), https://tc39.es/ecma262/#sec-createrealm
#pragma warning disable CS8618 // All properties are initialised in CreateIntrinsics
    internal Realm(out VM vm)
    {
        vm = new(this);

        // 1. Let realmRec be a new Realm Record.

        // 2. Perform CreateIntrinsics(realmRec).
        CreateIntrinsics(vm);

        // 3. Set realmRec.[[AgentSignifier]] to AgentSignifier(). 
        Agent = Agent.AgentSignifier();

        // 4. Set realmRec.[[GlobalObject]] to undefined.
        GlobalObject = Undefined.The;

        // FIXME: 5. Set realmRec.[[GlobalEnv]] to undefined.
        GlobalEnv = null;

        // FIXME: 6. Set realmRec.[[TemplateMap]] to a new empty List.

        // 7. Return realmRec.
    }
#pragma warning restore CS8618 // All properties are initialised in CreateIntrinsics

    // 9.3.2 CreateIntrinsics ( realmRec ), https://tc39.es/ecma262/#sec-createintrinsics
    private void CreateIntrinsics(VM vm)
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

        ObjectPrototype.Initialize(this, vm);
        ObjectConstructor.Initialize(this);

        FunctionPrototype.Initialize(vm);

        StringConstructor = new(FunctionPrototype);

        ErrorPrototype = new(ObjectPrototype);
        ErrorConstructor = new(FunctionPrototype);
        ErrorPrototype.Initialize(this, vm);
        ErrorConstructor.Initialize(this);

        EvalErrorPrototype = new(ErrorPrototype);
        EvalErrorConstructor = new(ErrorConstructor);
        EvalErrorPrototype.Initialize(EvalErrorConstructor, "EvalError");
        EvalErrorConstructor.Initialize(EvalErrorPrototype, "EvalError");

        RangeErrorPrototype = new(ErrorPrototype);
        RangeErrorConstructor = new(ErrorConstructor);
        RangeErrorPrototype.Initialize(RangeErrorConstructor, "RangeError");
        RangeErrorConstructor.Initialize(RangeErrorPrototype, "RangeError");

        ReferenceErrorPrototype = new(ErrorPrototype);
        ReferenceErrorConstructor = new(ErrorConstructor);
        ReferenceErrorPrototype.Initialize(ReferenceErrorConstructor, "ReferenceError");
        ReferenceErrorConstructor.Initialize(ReferenceErrorPrototype, "ReferenceError");

        SyntaxErrorPrototype = new(ErrorPrototype);
        SyntaxErrorConstructor = new(ErrorConstructor);
        SyntaxErrorPrototype.Initialize(SyntaxErrorConstructor, "SyntaxError");
        SyntaxErrorConstructor.Initialize(SyntaxErrorPrototype, "SyntaxError");

        TypeErrorPrototype = new(ErrorPrototype);
        TypeErrorConstructor = new(ErrorConstructor);
        TypeErrorPrototype.Initialize(TypeErrorConstructor, "TypeError");
        TypeErrorConstructor.Initialize(TypeErrorPrototype, "TypeError");

        URIErrorPrototype = new(ErrorPrototype);
        URIErrorConstructor = new(ErrorConstructor);
        URIErrorPrototype.Initialize(URIErrorConstructor, "URIError");
        URIErrorConstructor.Initialize(URIErrorPrototype, "URIError");

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
    private Completion SetDefaultGlobalBindings(VM vm)
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
            var defineResult = Object.DefinePropertyOrThrow(vm, GlobalObject, name, desc);
            if (defineResult.IsAbruptCompletion()) return defineResult;
        }

        // 3. Return global.
        return GlobalObject;
    }

    private Dictionary<string, Property> CreateGlobalProperties()
    {
        Dictionary<string, Property> globalProperties = new();

        // 19.1 Value Properties of the Global Object
        // 19.1.2 Infinity
        globalProperties.Add("Infinity", new Property(Number.Infinity, new(false, false, false)));

        // 19.1.4 undefined
        globalProperties.Add("undefined", new Property(Undefined.The, new(false, false, false)));

        // 19.3 Constructor Properties of the Global Object
        // 20.5.1 The Error Constructor, https://tc39.es/ecma262/#sec-error-constructor
        globalProperties.Add("Error", new Property(ErrorConstructor, new(true, false, true)));

        // 20.5.5.1 EvalError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-evalerror
        globalProperties.Add("EvalError", new Property(EvalErrorConstructor, new(true, false, true)));

        // 20.5.5.2 RangeError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-rangeerror
        globalProperties.Add("RangeError", new Property(RangeErrorConstructor, new(true, false, true)));

        // 20.5.5.3 ReferenceError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-referenceerror
        globalProperties.Add("ReferenceError", new Property(ReferenceErrorConstructor, new(true, false, true)));

        // 20.5.5.4 SyntaxError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-syntaxerror
        globalProperties.Add("SyntaxError", new Property(SyntaxErrorConstructor, new(true, false, true)));

        // 20.5.5.5 TypeError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-typeerror
        globalProperties.Add("TypeError", new Property(TypeErrorConstructor, new(true, false, true)));

        // 20.5.5.6 URIError, https://tc39.es/ecma262/#sec-native-error-types-used-in-this-standard-urierror
        globalProperties.Add("URIError", new Property(URIErrorConstructor, new(true, false, true)));

        // 20.1.1 The Object Constructor, https://tc39.es/ecma262/#sec-object-constructor
        globalProperties.Add("Object", new Property(ObjectConstructor, new(true, false, true)));

        // 22.1.1 The String Constructor, https://tc39.es/ecma262/#sec-string-constructor
        globalProperties.Add("String", new Property(StringConstructor, new(true, false, true)));

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
        var realm = new Realm(out vm);

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
        var globalObj = realm.SetDefaultGlobalBindings(vm);
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
    internal StringConstructor StringConstructor { get; private set; }
    internal ErrorPrototype ErrorPrototype { get; private set; }
    internal ErrorConstructor ErrorConstructor { get; private set; }
    internal NativeErrorPrototype EvalErrorPrototype { get; private set; }
    internal NativeErrorConstructor EvalErrorConstructor { get; private set; }
    internal NativeErrorPrototype RangeErrorPrototype { get; private set; }
    internal NativeErrorConstructor RangeErrorConstructor { get; private set; }
    internal NativeErrorPrototype ReferenceErrorPrototype { get; private set; }
    internal NativeErrorConstructor ReferenceErrorConstructor { get; private set; }
    internal NativeErrorPrototype SyntaxErrorPrototype { get; private set; }
    internal NativeErrorConstructor SyntaxErrorConstructor { get; private set; }
    internal NativeErrorPrototype TypeErrorPrototype { get; private set; }
    internal NativeErrorConstructor TypeErrorConstructor { get; private set; }
    internal NativeErrorPrototype URIErrorPrototype { get; private set; }
    internal NativeErrorConstructor URIErrorConstructor { get; private set; }
}
