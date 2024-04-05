using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.7.4 The for Statement, https://tc39.es/ecma262/#sec-for-statement
internal sealed class ForStatement : INode
{
    public ForStatement(INode? initializationExpression, INode? testExpression, INode? incrementExpression, INode iterationStatement)
    {
        InitializationExpression = initializationExpression;
        TestExpression = testExpression;
        IncrementExpression = incrementExpression;
        IterationStatement = iterationStatement;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    override public List<string> VarDeclaredNames()
    {
        // 1. Let names1 be BoundNames of VariableDeclarationList.
        List<string> names = new();
        if (InitializationExpression is VarStatement)
        {
            names.AddRange(InitializationExpression.BoundNames());
        }

        // 2. Let names2 be VarDeclaredNames of Statement.
        names.AddRange(IterationStatement.VarDeclaredNames());

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    override public List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be VarScopedDeclarations of VariableDeclarationList.
        List<INode> declarations = new();
        if (InitializationExpression is VarStatement)
        {
            declarations.AddRange(InitializationExpression.VarScopedDeclarations());
        }

        // 2. Let declarations2 be VarScopedDeclarations of Statement.
        declarations.AddRange(IterationStatement.VarScopedDeclarations());

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 14.7.4.2 Runtime Semantics: ForLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-forloopevaluation
    override public Completion Evaluate(VM vm)
    {
        if (InitializationExpression is VarStatement)
        {
            // FIXME: for loop execution with var statements
            throw new NotImplementedException();
        }
        else if (InitializationExpression is LetDeclaration or ConstDeclaration)
        {
            // FIXME: for loop execution with lexical declarations
            throw new NotImplementedException();
        }
        else
        {
            return EvaluationWithExpression(vm);
        }
    }

    public Completion EvaluationWithExpression(VM vm)
    {
        // 1. If the first Expression is present, then
        if (InitializationExpression is not null)
        {
            // a. Let exprRef be ? Evaluation of the first Expression.
            var exprRef = InitializationExpression.Evaluate(vm);
            if (exprRef.IsAbruptCompletion()) return exprRef;

            // b. Perform ? GetValue(exprRef).
            var exprValue = exprRef.Value.GetValue();
            if (exprValue.IsAbruptCompletion()) return exprValue;
        }

        // NOTE: These steps are implicit
        // 2. If the second Expression is present, let test be the second Expression; otherwise, let test be EMPTY.
        // 3. If the third Expression is present, let increment be the third Expression; otherwise, let increment be EMPTY.

        // 4. Return ? ForBodyEvaluation(test, increment, Statement, « », labelSet).
        return ForBodyEvaluation(vm);
    }

    // 14.7.4.3 ForBodyEvaluation ( test, increment, stmt, FIXME: perIterationBindings, FIXME: labelSet ), https://tc39.es/ecma262/#sec-forbodyevaluation
    private Completion ForBodyEvaluation(VM vm)
    {
        // 1. Let V be undefined.
        var V = (Value)Undefined.The;

        // FIXME: 2. Perform ? CreatePerIterationEnvironment(perIterationBindings).

        // 3. Repeat,
        while (true)
        {
            // a. If test is not EMPTY, then
            if (TestExpression is not null)
            {
                // i. Let testRef be ? Evaluation of test.
                var testRef = TestExpression.Evaluate(vm);
                if (testRef.IsAbruptCompletion()) return testRef;

                // ii. Let testValue be ? GetValue(testRef).
                var testValue = testRef.Value.GetValue();
                if (testValue.IsAbruptCompletion()) return testValue;

                // iii. If ToBoolean(testValue) is false, return V.
                if (!testValue.Value.ToBoolean().Value)
                {
                    return V;
                }
            }

            // b. Let result be Completion(Evaluation of stmt).
            var result = IterationStatement.Evaluate(vm);

            // c. If LoopContinues(result, labelSet) is false, return ? UpdateEmpty(result, V).
            if (!result.LoopContinues().Value)
            {
                result.UpdateEmpty(V);
                return result;
            }

            // d. If result.[[Value]] is not EMPTY, set V to result.[[Value]].
            if (!result.IsValueEmpty())
            {
                V = result.Value;
            }

            // FIXME: e. Perform ? CreatePerIterationEnvironment(perIterationBindings).

            // f. If increment is not EMPTY, then
            if (IncrementExpression is not null)
            {
                // i. Let incRef be ? Evaluation of increment.
                var incRef = IncrementExpression.Evaluate(vm);
                if (incRef.IsAbruptCompletion()) return incRef;

                // ii. Perform ? GetValue(incRef).
                var incValue = incRef.Value.GetValue();
                if (incValue.IsAbruptCompletion()) return incValue;
            }
        }
    }

    public INode? InitializationExpression { get; }
    public INode? TestExpression { get; }
    public INode? IncrementExpression { get; }
    public INode IterationStatement { get; }
}
