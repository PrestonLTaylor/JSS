using JSS.Lib.AST.Values;
using System.Diagnostics;

namespace JSS.Lib.Execution;

internal sealed class ScriptExecutionContext : ExecutionContext
{
    public ScriptExecutionContext(Realm realm) : base(realm)
    {
    }

    // 8.6.2.1 InitializeBoundName ( name, value, environment ), https://tc39.es/ecma262/#sec-initializeboundname
    static internal Completion InitializeBoundName(VM vm, string name, Value value, Value environment)
    {
        // 1. If environment is not undefined, then
        if (!environment.IsUndefined())
        {
            // a. Perform ! environment.InitializeBinding(name, value).
            var asEnvironment = environment.AsEnvironment();
            MUST(asEnvironment.InitializeBinding(vm, name, value));

            // b. Return UNUSED.
            return Empty.The;
        }
        else
        {
            // a. Let lhs be ? ResolveBinding(name).
            var lhs = ResolveBinding(vm, name);
            if (lhs.IsAbruptCompletion()) return lhs;

            // b. Return ? PutValue(lhs, value).
            return lhs.Value.PutValue(vm, value);
        }
    }

    // 9.4.2 ResolveBinding ( name [ , env ] ), https://tc39.es/ecma262/#sec-resolvebinding
    static public Completion ResolveBinding(VM vm, string name, Environment? env = null)
    {
        // 1. If env is not present or env is undefined, then
        if (env is null)
        {
            // NOTE: As we want to access the LexicalEnvironment there is an implicit assertion that
            // the execution context is a ScriptExecutionContext (as that's the only context that has a
            // lexical environment in the code base currently)
            // a. Set env to the running execution context's LexicalEnvironment.
            var scriptExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
            env = scriptExecutionContext.LexicalEnvironment;
        }

        // NOTE: This Assert is implicit
        // 2. Assert: env is an Environment Record.

        // FIXME: 3. If the source text matched by the syntactic production that is being evaluated is contained in strict mode code,
        // let strict be true; else let strict be false.

        // 4. Return ? GetIdentifierReference(env, name, strict).
        return Environment.GetIdentifierReference(env, name);
    }

    // 9.4.3 GetThisEnvironment ( ), https://tc39.es/ecma262/#sec-getthisenvironment
    static public Environment GetThisEnvironment(VM vm)
    {
        // 1. Let env be the running execution context's LexicalEnvironment.
        var env = (vm.CurrentExecutionContext as ScriptExecutionContext)!.LexicalEnvironment;

        // 2. Repeat,
        while (true)
        {
            // a. Let exists be env.HasThisBinding().
            var exists = env!.HasThisBinding();

            // b. If exists is true, return env.
            if (exists) return env;

            // c. Let outer be env.[[OuterEnv]].
            var outer = env.OuterEnv;

            // d. Assert: outer is not null.
            Assert(outer is not null, "d. Assert: outer is not null.");

            // e. Set env to outer.
            env = outer;
        }
    }

    // 9.4.4 ResolveThisBinding ( ), https://tc39.es/ecma262/#sec-resolvethisbinding
    static public Completion ResolveThisBinding(VM vm)
    {
        // 1. Let envRec be GetThisEnvironment().
        var envRec = GetThisEnvironment(vm);

        // 2. Return ? envRec.GetThisBinding().
        return envRec.GetThisBinding(vm);
    }

    public Environment? LexicalEnvironment { get; set; }
    public Environment? VariableEnvironment { get; set; }
    public Environment? PrivateEnvironment { get; set; }
}
