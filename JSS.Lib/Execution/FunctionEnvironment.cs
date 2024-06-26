﻿using JSS.Lib.AST.Values;
using System.Diagnostics;

namespace JSS.Lib.Execution;

enum ThisBindingStatus
{
    LEXICAL,
    INITIALIZED,
    UNINITIALIZED,
}

// 9.1.1.3 Function Environment Records, https://tc39.es/ecma262/#sec-function-environment-records
internal class FunctionEnvironment : DeclarativeEnvironment
{
    // 9.1.2.4 NewFunctionEnvironment ( F, newTarget ), https://tc39.es/ecma262/#sec-newfunctionenvironment
    public FunctionEnvironment(FunctionObject F, Object newTarget) : base(F.Environment)
    {
        // 1. Let env be a new Function Environment Record containing no bindings.
        // 2. Set env.[[FunctionObject]] to F.
        FunctionObject = F;

        // 3. If F.[[ThisMode]] is LEXICAL, set env.[[ThisBindingStatus]] to LEXICAL.
        if (F.ThisMode == ThisMode.LEXICAL)
        {
            ThisBindingStatus = ThisBindingStatus.LEXICAL;
        }
        // 4. Else, set env.[[ThisBindingStatus]] to UNINITIALIZED.
        else
        {
            ThisBindingStatus = ThisBindingStatus.UNINITIALIZED;
        }

        // 5. Set env.[[NewTarget]] to newTarget.
        NewTarget = newTarget;

        // NOTE: Performed in the base constructor call
        // 6. Set env.[[OuterEnv]] to F.[[Environment]].

        // 7. Return env.
    }

    // 9.1.1.3.1 BindThisValue ( V ), https://tc39.es/ecma262/#sec-bindthisvalue
    public Completion BindThisValue(VM vm, Value V)
    {
        // 1. Assert: envRec.[[ThisBindingStatus]] is not LEXICAL.
        Assert(ThisBindingStatus != ThisBindingStatus.LEXICAL, "1. Assert: envRec.[[ThisBindingStatus]] is not LEXICAL.");

        // 2. If envRec.[[ThisBindingStatus]] is INITIALIZED, throw a ReferenceError exception.
        if (ThisBindingStatus == ThisBindingStatus.INITIALIZED)
        {
            return ThrowReferenceError(vm, RuntimeErrorType.BindingThisToAlreadyThisInitializedEnvironment);
        }

        // 3. Set envRec.[[ThisValue]] to V.
        ThisValue = V;

        // 4. Set envRec.[[ThisBindingStatus]] to INITIALIZED.
        ThisBindingStatus = ThisBindingStatus.INITIALIZED;

        // 5. Return V.
        return V;
    }

    // 9.1.1.3.2 HasThisBinding ( ), https://tc39.es/ecma262/#sec-function-environment-records-hasthisbinding
    override public bool HasThisBinding()
    {
        // 1. If envRec.[[ThisBindingStatus]] is LEXICAL, return false; otherwise, return true.
        return ThisBindingStatus != ThisBindingStatus.LEXICAL;
    }

    // 9.1.1.3.4 GetThisBinding ( ), https://tc39.es/ecma262/#sec-function-environment-records-getthisbinding
    override public Completion GetThisBinding(VM vm)
    {
        // 1. Assert: envRec.[[ThisBindingStatus]] is not LEXICAL.
        Assert(ThisBindingStatus != ThisBindingStatus.LEXICAL, "1. Assert: envRec.[[ThisBindingStatus]] is not LEXICAL.");

        // 2. If envRec.[[ThisBindingStatus]] is UNINITIALIZED, throw a ReferenceError exception.
        if (ThisBindingStatus == ThisBindingStatus.UNINITIALIZED)
        {
            return ThrowReferenceError(vm, RuntimeErrorType.UninitializedThisValue);
        }

        // 3. Return envRec.[[ThisValue]].
        return ThisValue!;
    }

    public Value? ThisValue { get; private set; }
    public ThisBindingStatus ThisBindingStatus { get; private set; }
    public FunctionObject FunctionObject { get; }
    public Object NewTarget { get; }
}
