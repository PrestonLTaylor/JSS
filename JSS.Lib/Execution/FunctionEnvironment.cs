using JSS.Lib.AST.Values;
using Object = JSS.Lib.AST.Values.Object;

namespace JSS.Lib.Execution;

enum ThisBindingStatus
{
    LEXICAL,
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

    public FunctionObject FunctionObject { get; }
    public Object NewTarget { get; }
    public ThisBindingStatus ThisBindingStatus { get; }
}
