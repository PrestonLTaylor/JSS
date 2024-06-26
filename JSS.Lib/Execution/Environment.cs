﻿using JSS.Lib.AST.Values;

namespace JSS.Lib.Execution;

// 9.1 Environment Records, https://tc39.es/ecma262/#sec-environment-records
internal abstract class Environment : Value
{
    override public bool IsEnvironment() { return true; }
    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of Environment"); }

    // Abstract Methods of Environment Records, https://tc39.es/ecma262/#table-abstract-methods-of-environment-records
    virtual public bool HasBinding(string N) { throw new NotImplementedException(); }
    virtual public Completion CreateMutableBinding(VM vm, string N, bool D) { throw new NotImplementedException(); }
    virtual public Completion CreateImmutableBinding(VM vm, string N, bool S) { throw new NotImplementedException(); }
    virtual public Completion InitializeBinding(VM vm, string N, Value V) { throw new NotImplementedException(); }
    virtual public Completion SetMutableBinding(VM vm, string N, Value V, bool S) { throw new NotImplementedException(); }
    virtual public Completion GetBindingValue(VM vm, string N, bool S) { throw new NotImplementedException(); }
    virtual public Completion DeleteBinding(string N) { throw new NotImplementedException(); }
    abstract public bool HasThisBinding();
    virtual public bool HasSuperBinding() { throw new NotImplementedException(); }
    virtual public Value WithBaseObject() { throw new NotImplementedException(); }
    virtual public Completion GetThisBinding(VM vm) { throw new InvalidOperationException(); }

    // 9.1.2.1 GetIdentifierReference( env, name, strict ), https://tc39.es/ecma262/#sec-getidentifierreference
    static public Completion GetIdentifierReference(Environment? env, string name, bool strict)
    {
        // 1. If env is null, then
        if (env is null)
        {
            // a. Return the Reference Record { [[Base]]: UNRESOLVABLE, [[ReferencedName]]: name, [[Strict]]: strict, [[ThisValue]]: EMPTY }.
            return Reference.Unresolvable(name, strict, Empty.The);
        }

        // 2. Let exists be ? env.HasBinding(name).
        var exists = env.HasBinding(name);

        // 3. If exists is true, then
        if (exists)
        {
            // a. Return the Reference Record { [[Base]]: env, [[ReferencedName]]: name, [[Strict]]: strict, [[ThisValue]]: EMPTY }.
            return Reference.Resolvable(env, name, strict, Empty.The);
        }
        // 4. Else,
        else
        {
            // a. Let outer be env.[[OuterEnv]].
            var outer = env.OuterEnv;

            // b. Return ? GetIdentifierReference(outer, name, strict).
            return GetIdentifierReference(outer, name, strict);
        }
    }

    public Environment? OuterEnv { get; protected set; }
}
