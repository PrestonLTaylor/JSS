using JSS.Lib.Execution;
using System.Diagnostics;
using Environment = JSS.Lib.Execution.Environment;
using ExecutionContext = JSS.Lib.Execution.ExecutionContext;

namespace JSS.Lib.AST.Values;

// FIXME: Spec links for FunctionObject when FunctionObject is more fleshed out
internal sealed class FunctionObject : Object
{
    public FunctionObject(IReadOnlyList<Identifier> formalParameters, StatementList body, Environment env) : base(null)
    {
        FormalParameters = formalParameters;
        ECMAScriptCode = body;
        Environment = env;
    }

    override public bool IsFunction() { return true; }
    public override bool HasInternalCall() { return true; }
    override public ValueType Type() {  return ValueType.Function; }

    // 10.2.9 SetFunctionName ( F, name [ , prefix ] ), https://tc39.es/ecma262/#sec-setfunctionname
    public void SetFunctionName(string name, string prefix = "")
    {
        // 1. Assert: FIXME: (F is an extensible object) that does not have a "name" own property.
        Debug.Assert(!DataProperties.ContainsKey("name"));

        // FIXME: 2. If name is a Symbol, then
        // FIXME: a. Let description be name's [[Description]] value.
        // FIXME: b. If description is undefined, set name to the empty String.
        // FIXME: c. Else, set name to the string-concatenation of "[", description, and "]".
        // FIXME: 3. Else if name is a Private Name, then
        // FIXME: a. Set name to name.[[Description]].
        // FIXME: 4. If F has an [[InitialName]] internal slot, then
        // FIXME: a. Set F.[[InitialName]] to name.

        // 5. If prefix is present, then
        if (!string.IsNullOrEmpty(prefix))
        {
            // a. Set name to the string-concatenation of prefix, the code unit 0x0020 (SPACE), and name.
            name = prefix + " " + name;

            // FIXME: b. If F has an [[InitialName]] internal slot, then
            // FIXME: i. Optionally, set F.[[InitialName]] to name.
        }

        // 6. Perform ! DefinePropertyOrThrow(F, "name", PropertyDescriptor { [[Value]]: name, [[Writable]]: false, [[Enumerable]]: false, [[Configurable]]: true }).
        var result = DefinePropertyOrThrow(this, "name", new Property(new String(name), new Attributes(false, false, true)));
        Debug.Assert(result.IsNormalCompletion());

        // 7. Return unused.
    }

    public IReadOnlyList<Identifier> FormalParameters { get; }
    public StatementList ECMAScriptCode { get; }
    public Environment Environment { get; }
}
