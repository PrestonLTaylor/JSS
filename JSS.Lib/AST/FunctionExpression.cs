using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using JSS.Lib.Runtime;

namespace JSS.Lib.AST;

// 15.2 Function Definitions, https://tc39.es/ecma262/#sec-function-definitions
internal sealed class FunctionExpression : IExpression
{
    public FunctionExpression(string? identifier, List<Identifier> parameters, StatementList body, bool isStrict)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
        IsStrict = isStrict;
    }

    // 15.2.6 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-definitions-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // 1. Return InstantiateOrdinaryFunctionExpression of FunctionExpression.
        if (Identifier is null)
        {
            return InstantiateOrdinaryFunctionExpressionWithNoName(vm);
        }
        else
        {
            return InstantiateOrdinaryFunctionExpression(vm);
        }
    }

    // 15.2.5 Runtime Semantics: InstantiateOrdinaryFunctionExpression, https://tc39.es/ecma262/#sec-runtime-semantics-instantiateordinaryfunctionexpression
    private Completion InstantiateOrdinaryFunctionExpressionWithNoName(VM vm, string? name = null)
    {
        // 1. If name is not present, set name to "".
        name ??= "";

        // 2. Let env be the LexicalEnvironment of the running execution context.
        var env = (vm.CurrentExecutionContext as ScriptExecutionContext)!.LexicalEnvironment;

        // FIXME: 3. Let privateEnv be the running execution context's PrivateEnvironment.

        // FIXME: 4. Let sourceText be the source text matched by FunctionExpression.

        // 5. Let closure be OrdinaryFunctionCreate(%Function.prototype%, sourceText, FormalParameters, FunctionBody, NON-LEXICAL-THIS, env, privateEnv).
        var closure = FunctionObject.OrdinaryFunctionCreate(vm.FunctionPrototype, Parameters, Body, LexicalThisMode.NON_LEXICAL_THIS, env!, vm.IsStrict || IsStrict);

        // 6. Perform SetFunctionName(closure, name).
        closure.SetFunctionName(vm, name);

        // 7. Perform MakeConstructor(closure).
        closure.MakeConstructor(vm);

        // 8. Return closure.
        return closure;
    }

    private Completion InstantiateOrdinaryFunctionExpression(VM vm)
    {
        // 1. Assert: name is not present.
        // 2. Set name to StringValue of BindingIdentifier.
        var name = Identifier!;

        // 3. Let outerEnv be the running execution context's LexicalEnvironment.
        var outerEnv = (vm.CurrentExecutionContext as ScriptExecutionContext)!.LexicalEnvironment;

        // 4. Let funcEnv be NewDeclarativeEnvironment(outerEnv).
        var funcEnv = new DeclarativeEnvironment(outerEnv);

        // 5. Perform ! funcEnv.CreateImmutableBinding(name, false).
        MUST(funcEnv.CreateImmutableBinding(vm, name, false));

        // FIXME: 6. Let privateEnv be the running execution context's PrivateEnvironment.

        // FIXME: 7. Let sourceText be the source text matched by FunctionExpression.

        // 8. Let closure be OrdinaryFunctionCreate(%Function.prototype%, sourceText, FormalParameters, FunctionBody, NON-LEXICAL-THIS, funcEnv, privateEnv).
        var closure = FunctionObject.OrdinaryFunctionCreate(vm.FunctionPrototype, Parameters, Body, LexicalThisMode.NON_LEXICAL_THIS, funcEnv, vm.IsStrict || IsStrict);

        // 9. Perform SetFunctionName(closure, name).
        closure.SetFunctionName(vm, name);

        // 10. Perform MakeConstructor(closure).
        closure.MakeConstructor(vm);

        // 11. Perform ! funcEnv.InitializeBinding(name, closure).
        MUST(funcEnv.InitializeBinding(vm, name, closure));

        // 12. Return closure.
        return closure;
    }

    public string? Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public StatementList Body { get; }
    public bool IsStrict { get; }
}
