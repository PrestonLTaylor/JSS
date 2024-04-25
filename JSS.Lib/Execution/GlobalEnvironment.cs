using JSS.Lib.AST.Values;
using System.Diagnostics;

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

    // 9.1.1.4.2 CreateMutableBinding ( N, D ), https://tc39.es/ecma262/#sec-global-environment-records-createimmutablebinding-n-s
    override public Completion CreateMutableBinding(VM vm, string N, bool D)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, throw a TypeError exception.
        if (DeclarativeRecord.HasBinding(N)) return ThrowTypeError(vm, RuntimeErrorType.RedeclarationOfMutableBinding, N);

        // 3. Return ! DclRec.CreateMutableBinding(N, D).
        return MUST(DeclarativeRecord.CreateMutableBinding(vm, N, D));
    }

    // 9.1.1.4.3 CreateImmutableBinding ( N, S ), https://tc39.es/ecma262/#sec-global-environment-records-createimmutablebinding-n-s
    override public Completion CreateImmutableBinding(VM vm, string N, bool S)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, throw a TypeError exception.
        if (DeclarativeRecord.HasBinding(N)) return ThrowTypeError(vm, RuntimeErrorType.RedeclarationOfImmutableBinding, N);

        // 3. Return ! DclRec.CreateImmutableBinding(N, S).
        return MUST(DeclarativeRecord.CreateImmutableBinding(vm, N, S));
    }

    // 9.1.1.4.4 InitializeBinding ( N, V ), https://tc39.es/ecma262/#sec-global-environment-records-initializebinding-n-v
    override public Completion InitializeBinding(VM vm, string N, Value V)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ! DclRec.InitializeBinding(N, V).
            return MUST(DeclarativeRecord.InitializeBinding(vm, N, V));
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
    override public Completion GetBindingValue(VM vm, string N, bool S)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ? DclRec.GetBindingValue(N, S).
            return DeclarativeRecord.GetBindingValue(vm, N, S);
        }

        // 3. Let ObjRec be envRec.[[ObjectRecord]].
        // 4. Return ? ObjRec.GetBindingValue(N, S).
        return ObjectRecord.GetBindingValue(vm, N, S);
    }

    // 9.1.1.4.7 DeleteBinding ( N ), https://tc39.es/ecma262/#sec-global-environment-records-deletebinding-n
    public override Completion DeleteBinding(string N)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].

        // 2. If ! DclRec.HasBinding(N) is true, then
        if (DeclarativeRecord.HasBinding(N))
        {
            // a. Return ! DclRec.DeleteBinding(N).
            return MUST(DeclarativeRecord.DeleteBinding(N));
        }

        // 3. Let ObjRec be envRec.[[ObjectRecord]].

        // 4. Let globalObject be ObjRec.[[BindingObject]].

        // 5. Let existingProp be ? HasOwnProperty(globalObject, N).
        var existingProp = Object.HasOwnProperty(ObjectRecord.BindingObject, N);
        if (existingProp.IsAbruptCompletion()) return existingProp;

        // 6. If existingProp is true, then
        var asBoolean = existingProp.Value.AsBoolean();
        if (asBoolean)
        {
            // a. Let status be ? ObjRec.DeleteBinding(N).
            var status = ObjectRecord.DeleteBinding(N);
            if (status.IsAbruptCompletion()) return status;

            // b. If status is true and envRec.[[VarNames]] contains N, then
            if (status.Value.AsBoolean() && VarNames.Contains(N))
            {
                // i. Remove N from envRec.[[VarNames]].
                VarNames.Remove(N);
            }

            // c. Return status.
            return status;
        }

        // 7. Return true.
        return true;
    }

    // 9.1.1.4.8 HasThisBinding ( ), https://tc39.es/ecma262/#sec-global-environment-records-hasthisbinding
    public override bool HasThisBinding()
    {
        // 1. Return true.
        return true;
    }

    // 9.1.1.4.10 WithBaseObject ( ), https://tc39.es/ecma262/#sec-global-environment-records-withbaseobject
    public override Value WithBaseObject()
    {
        // 1. Return undefined.
        return Undefined.The;
    }

    // 9.1.1.4.11 GetThisBinding ( ), https://tc39.es/ecma262/#sec-global-environment-records-getthisbinding
    public override Completion GetThisBinding(VM _)
    {
        // 1. Return envRec.[[GlobalThisValue]].
        return GlobalThisValue;
    }

    // 9.1.1.4.12 HasVarDeclaration ( N ), https://tc39.es/ecma262/#sec-hasvardeclaration
    public bool HasVarDeclaration(string N)
    {
        // 1. Let varDeclaredNames be envRec.[[VarNames]].
        // 2. If varDeclaredNames contains N, return true.
        // 3. Return false.
        return VarNames.Contains(N);
    }

    // 9.1.1.4.13 HasLexicalDeclaration ( N ), https://tc39.es/ecma262/#sec-haslexicaldeclaration
    public bool HasLexicalDeclaration(string N)
    {
        // 1. Let DclRec be envRec.[[DeclarativeRecord]].
        // 2. Return ! DclRec.HasBinding(N).
        return DeclarativeRecord.HasBinding(N);
    }

    // 9.1.1.4.14 HasRestrictedGlobalProperty ( N ), https://tc39.es/ecma262/#sec-hasrestrictedglobalproperty
    public Completion HasRestrictedGlobalProperty(string N)
    {
        // 1. Let ObjRec be envRec.[[ObjectRecord]].
        // 2. Let globalObject be ObjRec.[[BindingObject]].
        var globalObject = ObjectRecord.BindingObject;

        // 3. Let existingProp be ? globalObject.[[GetOwnProperty]](N).
        var existingProp = globalObject.GetOwnProperty(N);
        if (existingProp.IsAbruptCompletion()) return existingProp;

        // 4. If existingProp is undefined, return false.
        if (existingProp.Value.IsUndefined()) return false;

        // 5. If existingProp.[[Configurable]] is true, return false.
        var asProperty = existingProp.Value.AsProperty();
        if (asProperty.Attributes.Configurable) return false;

        // 6. Return true.
        return true;
    }

    // 9.1.1.4.15 CanDeclareGlobalVar ( N ), https://tc39.es/ecma262/#sec-candeclareglobalvar
    public Completion CanDeclareGlobalVar(string N)
    {
        // 1. Let ObjRec be envRec.[[ObjectRecord]].
        // 2. Let globalObject be ObjRec.[[BindingObject]].
        var globalObject = ObjectRecord.BindingObject;

        // 3. Let hasProperty be ? HasOwnProperty(globalObject, N).
        var hasProperty = Object.HasOwnProperty(globalObject, N);
        if (hasProperty.IsAbruptCompletion()) return hasProperty;

        // 4. If hasProperty is true, return true.
        var asBoolean = hasProperty.Value.AsBoolean();
        if (asBoolean.Value) return true;

        // FIXME: 5. Return ? IsExtensible(globalObject).
        return true;
    }

    // 9.1.1.4.16 CanDeclareGlobalFunction ( N ), https://tc39.es/ecma262/#sec-candeclareglobalfunction
    public Completion CanDeclareGlobalFunction(string N)
    {
        // 1. Let ObjRec be envRec.[[ObjectRecord]].
        // 2. Let globalObject be ObjRec.[[BindingObject]].
        var globalObject = ObjectRecord.BindingObject;

        // 3. Let existingProp be ? globalObject.[[GetOwnProperty]](N).
        var existingProp = globalObject.GetOwnProperty(N);
        if (existingProp.IsAbruptCompletion()) return existingProp;

        // 4. If existingProp is undefined, FIXME: return ? IsExtensible(globalObject).
        if (existingProp.Value.IsUndefined()) return true;

        // 5. If existingProp.[[Configurable]] is true, return true.
        var asProperty = existingProp.Value.AsProperty();
        if (asProperty.Attributes.Configurable) return true;

        // FIXME: 6. If IsDataDescriptor(existingProp) is true and existingProp has attribute values { [[Writable]]: true, [[Enumerable]]: true }, return true.
        // 7. Return false.
        return false;
    }

    // 9.1.1.4.17 CreateGlobalVarBinding ( N, D ), https://tc39.es/ecma262/#sec-createglobalvarbinding
    public Completion CreateGlobalVarBinding(VM vm, string N, bool D)
    {
        // 1. Let ObjRec be envRec.[[ObjectRecord]].
        // 2. Let globalObject be ObjRec.[[BindingObject]].
        var globalObject = ObjectRecord.BindingObject;

        // 3. Let hasProperty be ? HasOwnProperty(globalObject, N).
        var hasProperty = Object.HasOwnProperty(globalObject, N);
        if (hasProperty.IsAbruptCompletion()) return hasProperty;

        // FIXME: 4. Let extensible be ? IsExtensible(globalObject).
        // 5. If hasProperty is false FIXME: (and extensible is true), then
        var asBoolean = hasProperty.Value.AsBoolean();
        if (!asBoolean.Value)
        {
            // a. Perform ? ObjRec.CreateMutableBinding(N, D).
            var createResult = ObjectRecord.CreateMutableBinding(vm, N, D);
            if (createResult.IsAbruptCompletion()) return createResult;

            // b. Perform ? ObjRec.InitializeBinding(N, undefined).
            var initResult = ObjectRecord.InitializeBinding(vm, N, Undefined.The);
            if (initResult.IsAbruptCompletion()) return initResult;
        }

        // 6. If envRec.[[VarNames]] does not contain N, then
        if (!VarNames.Contains(N))
        {
            // a. Append N to envRec.[[VarNames]].
            VarNames.Add(N);
        }

        // 7. Return unused.
        return Empty.The;
    }

    // 9.1.1.4.18 CreateGlobalFunctionBinding ( N, V, D ), https://tc39.es/ecma262/#sec-createglobalfunctionbinding
    public Completion CreateGlobalFunctionBinding(VM vm, string N, Value V, bool D)
    {
        // 1. Let ObjRec be envRec.[[ObjectRecord]].
        // 2. Let globalObject be ObjRec.[[BindingObject]].
        var globalObject = ObjectRecord.BindingObject;

        // 3. Let existingProp be ? globalObject.[[GetOwnProperty]](N).
        var existingProp = globalObject.GetOwnProperty(N);
        if (existingProp.IsAbruptCompletion()) return existingProp;

        // 4. If existingProp is undefined or existingProp.[[Configurable]] is true, then
        Property desc;
        if (existingProp.Value.IsUndefined() || existingProp.Value.AsProperty().Attributes.Configurable)
        {
            // a. Let desc be the PropertyDescriptor { [[Value]]: V, [[Writable]]: true, [[Enumerable]]: true, [[Configurable]]: D }.
            desc = new Property(V, new(true, true, D));
        }
        // 5. Else,
        else
        {
            // FIXME: Should have no attributes
            // a. Let desc be the PropertyDescriptor { [[Value]]: V }.
            desc = new Property(V, new(false, false, false));
        }

        // 6. Perform ? DefinePropertyOrThrow(globalObject, N, desc).
        var defineResult = Object.DefinePropertyOrThrow(vm, globalObject, N, desc);
        if (defineResult.IsAbruptCompletion()) return defineResult;

        // 7. Perform ? Set(globalObject, N, V, false).
        var setResult = Object.Set(vm, globalObject, N, V, false);
        if (setResult.IsAbruptCompletion()) return setResult;

        // 8. If envRec.[[VarNames]] does not contain N, then
        if (!VarNames.Contains(N))
        {
            // a. Append N to envRec.[[VarNames]].
            VarNames.Add(N);
        }

        // 9. Return unused.
        return Empty.The;
    }

    public ObjectEnvironment ObjectRecord { get; }
    public Object GlobalThisValue { get; }
    public DeclarativeEnvironment DeclarativeRecord { get; }
    public List<string> VarNames { get; }
}
