using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 15.3 Arrow Function Definitions, https://tc39.es/ecma262/#sec-arrow-function-definitions
internal sealed class ArrowFunctionExpression : IExpression
{
    public ArrowFunctionExpression(List<Identifier> parameters, INode body, bool isStrict)
    {
        Parameters = parameters;
        Body = body;
        IsStrict = isStrict;
    }


    // 15.3.4 Runtime Semantics: InstantiateArrowFunctionExpression, https://tc39.es/ecma262/#sec-runtime-semantics-instantiatearrowfunctionexpression
    private Completion InstantiateArrowFunctionExpression(VM vm, string name = "")
    {
        // 1. If name is not present, set name to "".

        // 2. Let env be the LexicalEnvironment of the running execution context.
        var scriptExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
        var env = scriptExecutionContext.LexicalEnvironment;

        // FIXME: 3. Let privateEnv be the running execution context's PrivateEnvironment.
        // FIXME: 4. Let sourceText be the source text matched by ArrowFunction.

        // 5. Let closure be OrdinaryFunctionCreate(%Function.prototype%, sourceText, ArrowParameters, ConciseBody, lexical-this, env, privateEnv).
        var closure = FunctionObject.OrdinaryFunctionCreate(vm.FunctionPrototype, Parameters, Body, LexicalThisMode.LEXICAL_THIS, env!, IsStrict);

        // 6. Perform SetFunctionName(closure, name).
        FunctionObject.SetFunctionName(vm, closure, name);

        // 7. Return closure.
        return closure;
    }

    // 15.3.5 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-arrow-function-definitions-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        // 1. Return InstantiateArrowFunctionExpression of ArrowFunction.
        return InstantiateArrowFunctionExpression(vm);
    }

    public List<Identifier> Parameters { get; }
    public INode Body { get; }
    public bool IsStrict { get; }
}
