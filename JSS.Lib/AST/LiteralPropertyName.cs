using JSS.Lib.AST.Literal;
using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// LiteralPropertyName, https://tc39.es/ecma262/#prod-LiteralPropertyName
internal sealed class LiteralPropertyName : INode
{
    public LiteralPropertyName(INode literal)
    {
        Literal = literal;
    }

    // 13.2.5.4 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-object-initializer-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        if (vm.CancellationToken.IsCancellationRequested)
        {
            return ThrowCancellationError(vm);
        }

        if (Literal is Identifier)
        {
            // 1. Return StringValue of IdentifierName.
            var identifier = Literal as Identifier;
            return identifier!.Name;
        }
        else if (Literal is StringLiteral)
        {
            // 1. Return the SV of StringLiteral.
            var stringLiteral = Literal as StringLiteral;
            return stringLiteral!.Value;
        }
        else if (Literal is NumericLiteral)
        {
            // 1. Let nbr be the NumericValue of NumericLiteral.
            var numericLiteral = Literal as NumericLiteral;
            var nbr = new Number(numericLiteral!.Value);

            // 2. Return ! ToString(nbr).
            return MUST(nbr.ToStringJS(vm));
        }

        Assert(false, $"{nameof(LiteralPropertyName)} created with invalid literal node");
        return Empty.The;
    }

    public INode Literal { get; }
}
