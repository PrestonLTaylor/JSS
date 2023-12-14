using JSS.Lib.AST.Values;
using Environment = JSS.Lib.Execution.Environment;

namespace JSS.Lib.AST;

// 15.2 Function Definitions, https://tc39.es/ecma262/#prod-FunctionDeclaration
internal sealed class FunctionDeclaration : Declaration
{
    // FIXME: Early errors for parameters having the same name, https://tc39.es/ecma262/#sec-function-definitions-static-semantics-early-errors
    public FunctionDeclaration(string identifier, List<Identifier> parameters, StatementList body)
    {
        Identifier = identifier;
        Parameters = parameters;
        Body = body;
    }

    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    override public List<string> BoundNames()
    {
        // 1. Return the BoundNames of BindingIdentifier.
        return new List<string> { Identifier };
    }

    // 8.6.1 Runtime Semantics: InstantiateFunctionObject, https://tc39.es/ecma262/#sec-runtime-semantics-instantiatefunctionobject
    public FunctionObject InstantiateFunctionObject(Environment env)
    {
        // NOTE/FIXME: This is the steps for InstantiateOrdinaryFunctionObject

        // 1. Let name be StringValue of BindingIdentifier.

        // FIXME: 2. Let sourceText be the source text matched by FunctionDeclaration.

        // FIXME: 3. Let F be OrdinaryFunctionCreate(%Function.prototype%, sourceText, FormalParameters, FunctionBody, non-lexical-this, env, privateEnv).
        var F = new FunctionObject(Parameters, Body);

        // 4. Perform SetFunctionName(F, name).
        F.SetFunctionName(Identifier);

        // FIXME: 5. Perform MakeConstructor(F).

        // 6. Return F.
        return F;
    }

    // FIXME: 15.2.6 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-definitions-runtime-semantics-evaluation

    public string Identifier { get; }
    public IReadOnlyList<Identifier> Parameters { get; }
    public StatementList Body { get; }
}
