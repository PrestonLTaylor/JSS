using JSS.Lib.Execution;

namespace JSS.Lib.AST.Literal;

internal sealed class ArrayLiteral : IExpression
{
    public ArrayLiteral(List<IExpression?> elements)
    {
        Elements = elements;
    }

    // 13.2.4.2 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-array-initializer-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let array be ! ArrayCreate(0).
        var array = MUST(Array.ArrayCreate(vm, 0));

        // 2. Perform ? ArrayAccumulation of ElementList with arguments array and 0.
        var accumulationResult = ArrayAccumulation(vm, array);
        if (accumulationResult.IsAbruptCompletion()) return accumulationResult;

        // 3. Return array.
        return array;
    }

    // 13.2.4.1 Runtime Semantics: ArrayAccumulation, https://tc39.es/ecma262/#sec-runtime-semantics-arrayaccumulation
    private Completion ArrayAccumulation(VM vm, Array array)
    {
        // FIXME/NOTE: This doesn't follow the spec steps exactly, however, this should fully mimic the behaviour.
        var nextIndex = 0;
        foreach (var element in Elements)
        {
            // 1. If Elision is present, then
            if (element is null)
            {
                // 1. Let len be nextIndex + 1.
                var len = nextIndex + 1;

                // 2. Perform ? Set(array, "length", 𝔽(len), true).
                var setResult = Object.Set(vm, array, "length", len, true);
                if (setResult.IsAbruptCompletion()) return setResult;

                // 3. NOTE: The above step throws if len exceeds 2**32 - 1.

                // 4. Return len.
                // a. Set nextIndex to ? ArrayAccumulation of Elision with arguments array and nextIndex.
                nextIndex = len;
            }
            else
            {
                // 2. Let initResult be ? Evaluation of AssignmentExpression.
                var initResult = element.Evaluate(vm);
                if (initResult.IsAbruptCompletion()) return initResult;

                // 3. Let initValue be ? GetValue(initResult).
                var initValue = initResult.Value.GetValue(vm);

                // 4. Perform ! CreateDataPropertyOrThrow(array, ! ToString(𝔽(nextIndex)), initValue).
                MUST(Object.CreateDataPropertyOrThrow(vm, array, nextIndex.ToString(), initValue.Value));

                // 5. Return nextIndex + 1.
                nextIndex++;
            }
        }

        // Other steps return the index in the happy path, so we return nextIndex here.
        return nextIndex;
    }

    public IReadOnlyList<IExpression?> Elements { get; }
}
