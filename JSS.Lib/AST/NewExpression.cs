using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using String = JSS.Lib.AST.Values.String;

namespace JSS.Lib.AST;

// 13.3.5 The new Operator, https://tc39.es/ecma262/#sec-new-operator
internal sealed class NewExpression : IExpression
{
    public NewExpression(IExpression expression, IReadOnlyList<IExpression> arguments)
    {
        Expression = expression;
        Arguments = arguments;
    }

    // 13.3.5.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-new-operator-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Return ? EvaluateNew(MemberExpression, Arguments).
        return EvaluateNew(vm);
    }

    // 13.3.5.1.1 EvaluateNew ( constructExpr, arguments ), https://tc39.es/ecma262/#sec-evaluatenew
    private Completion EvaluateNew(VM vm)
    {
        // 1. Let ref be ? Evaluation of constructExpr.
        var constructorRef = Expression.Evaluate(vm);
        if (constructorRef.IsAbruptCompletion()) return constructorRef;

        // 2. Let constructor be ? GetValue(ref).
        var constructor = constructorRef.Value.GetValue();
        if (constructor.IsAbruptCompletion()) return constructor;

        // FIXME: 3. If arguments is EMPTY, then
        // a. Let argList be a new empty List.
        var argList = new List();

        // FIXME: 4. Else,
        // FIXME: a. Let argList be ? ArgumentListEvaluation of arguments.
        foreach (var argument in Arguments)
        {
            var argRef = argument.Evaluate(vm);
            if (argRef.IsAbruptCompletion()) return argRef;

            var argValue = argRef.Value.GetValue();
            if (argValue.IsAbruptCompletion()) return argValue;
            argList.Add(argValue.Value);
        }

        // 5. If IsConstructor(constructor) is false, FIXME: throw a TypeError exception.
        if (!constructor.Value.IsConstructor())
        {
            return Completion.ThrowCompletion("Tried to construct from a non-constructable");
        }

        // 6. Return ? Construct(constructor, argList).
        var constructable = constructor.Value.AsConstructable();
        return constructable.Construct(vm, argList);
    }

    public IExpression Expression { get; }
    public IReadOnlyList<IExpression> Arguments { get; }
}
