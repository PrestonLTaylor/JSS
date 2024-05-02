using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using System.Diagnostics;

namespace JSS.Lib.AST;

// 13.3.6 Function Calls, https://tc39.es/ecma262/#sec-function-calls
internal sealed class CallExpression : IExpression
{
    public CallExpression(IExpression lhs, List<IExpression> arguments)
    {
        Lhs = lhs;
        Arguments = arguments;
    }

    // 13.3.6.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-function-calls-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let expr be the CallMemberExpression that is covered by CoverCallExpressionAndAsyncArrowHead.
        // 2. Let memberExpr be the MemberExpression of expr.
        // 3. Let arguments be the Arguments of expr.
        // 4. Let ref be ? Evaluation of memberExpr.
        var lref = Lhs.Evaluate(vm);
        if (lref.IsAbruptCompletion()) return lref;

        // 5. Let func be ? GetValue(ref).
        var func = lref.Value.GetValue(vm);
        if (func.IsAbruptCompletion()) return func;

        // 6. If ref is a Reference Record, FIXME: (IsPropertyReference(ref) is false), and ref.[[ReferencedName]] is "eval", then
        var asReference = lref.Value as Reference;
        if (asReference is not null && asReference.ReferencedName == "eval")
        {
            // FIXME: a.If SameValue(func, % eval %) is true, then
            // FIXME: i.Let argList be? ArgumentListEvaluation of arguments.
            // FIXME: ii.If argList has no elements, return undefined.
            // FIXME: iii.Let evalArg be the first element of argList.
            // FIXME: iv.If the source text matched by this CallExpression is strict mode code, let strictCaller be true.Otherwise let strictCaller be false.
            // FIXME: v.Return ? PerformEval(evalArg, strictCaller, true).
            throw new NotImplementedException();
        }

        // 7. Let thisCall be this CallExpression.

        // FIXME: 8. Let tailCall be IsInTailPosition(thisCall).

        // 9. Return ? EvaluateCall(func, ref, arguments, tailCall).
        return EvaluateCall(vm, func.Value, lref.Value);
    }

    // 13.3.6.2 EvaluateCall ( func, ref, arguments, FIXME: tailPosition ), https://tc39.es/ecma262/#sec-evaluatecall
    private Completion EvaluateCall(VM vm, Value func, Value lref)
    {
        // 1. If ref is a Reference Record, then
        Value thisValue;
        if (lref.IsReference())
        {
            // a. If IsPropertyReference(ref) is true, then
            var asReference = lref.AsReference();
            if (asReference.IsPropertyReference())
            {
                // i. Let thisValue be GetThisValue(ref).
                thisValue = asReference.GetThisValue();
            }
            // b. Else,
            else
            {
                // i. Let refEnv be ref.[[Base]].
                var refEnv = asReference.Base;

                // ii. Assert: refEnv is an Environment Record.
                Assert(refEnv is Environment, "Assert: refEnv is an Environment Record.");

                // iii. Let thisValue be refEnv.WithBaseObject().
                var asEnvironment = refEnv as Environment;
                thisValue = asEnvironment!.WithBaseObject();
            }
        }
        // 2. Else,
        else
        {
            // a. Let thisValue be undefined.
            thisValue = Undefined.The;
        }

        // FIXME: Make a ArgumentList class
        // 3. Let argList be ? ArgumentListEvaluation of arguments.
        var argList = ArgumentListEvaluation(vm);
        if (argList.IsAbruptCompletion()) return argList.Completion;

        // 4. If func is not an Object, throw a TypeError exception.
        if (!func.IsObject())
        {
            return ThrowTypeError(vm, RuntimeErrorType.CallingANonFunction, func.Type());
        }

        // 5. If IsCallable(func) is false, throw a TypeError exception.
        if (!func.IsCallable())
        {
            return ThrowTypeError(vm, RuntimeErrorType.CallingANonFunction, func.Type());
        }

        // FIXME: 6. If tailPosition is true, perform PrepareForTailCall().

        // 7. Return ? Call(func, thisValue, argList).
        return Object.Call(vm, func, thisValue, argList.Value);
    }

    // 13.3.8.1 Runtime Semantics: ArgumentListEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-argumentlistevaluation
    private AbruptOr<List> ArgumentListEvaluation(VM vm)
    {
        // 1. Let precedingArgs be ? ArgumentListEvaluation of ArgumentList.
        List precedingArgs = new();

        foreach (var argument in Arguments)
        {
            // 2. Let ref be ? Evaluation of AssignmentExpression.
            var aref = argument.Evaluate(vm);
            if (aref.IsAbruptCompletion()) return aref;

            // 3. Let arg be ? GetValue(ref).
            var arg = aref.Value.GetValue(vm);
            if (arg.IsAbruptCompletion()) return arg;

            // 4. Return the list-concatenation of precedingArgs and « arg ».
            precedingArgs.Add(arg.Value);
        }

        return precedingArgs;
    }

    public IExpression Lhs { get; }
    public IReadOnlyList<IExpression> Arguments { get; }
}
