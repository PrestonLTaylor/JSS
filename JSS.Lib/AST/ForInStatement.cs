using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// 14.7.5 The for-in, for-of, and for-await-of Statements, https://tc39.es/ecma262/#sec-for-in-and-for-of-statements
internal sealed class ForInStatement : INode, IBreakableStatement
{
    public ForInStatement(Identifier identifier, IExpression expression, INode iterationStatement)
    {
        Identifier = identifier;
        Expression = expression;
        IterationStatement = iterationStatement;
    }

    // 8.2.6 Static Semantics: VarDeclaredNames, https://tc39.es/ecma262/#sec-static-semantics-vardeclarednames
    public override List<string> VarDeclaredNames()
    {
        // 1. Let names1 be the BoundNames of ForBinding.
        var names = Identifier.BoundNames();

        // 2. Let names2 be the VarDeclaredNames of Statement.
        names.AddRange(IterationStatement.VarDeclaredNames());

        // 3. Return the list-concatenation of names1 and names2.
        return names;
    }

    // 8.2.7 Static Semantics: VarScopedDeclarations, https://tc39.es/ecma262/#sec-static-semantics-varscopeddeclarations
    public override List<INode> VarScopedDeclarations()
    {
        // 1. Let declarations1 be « ForBinding ».
        var declarations = new List<INode>() { Identifier };

        // 2. Let declarations2 be VarScopedDeclarations of Statement.
        declarations.AddRange(IterationStatement.VarScopedDeclarations());

        // 3. Return the list-concatenation of declarations1 and declarations2.
        return declarations;
    }

    // 14.7.5.5 Runtime Semantics: ForInOfLoopEvaluation, https://tc39.es/ecma262/#sec-runtime-semantics-forinofloopevaluation
    public Completion EvaluateFromLabelled(VM vm)
    {
        // 1. Let keyResult be ? ForIn/OfHeadEvaluation(« », Expression, ENUMERATE).
        var keyResult = ForInOfHeadEvaluation(vm);
        if (keyResult.IsAbruptCompletion()) return keyResult.Completion;

        // 2. Return ? ForIn/OfBodyEvaluation(LeftHandSideExpression, Statement, keyResult, ENUMERATE, ASSIGNMENT, labelSet).
        return ForInOfBodyEvaluation(vm, keyResult.Value);
    }

    // 14.7.5.6 ForIn/OfHeadEvaluation ( FIXME: uninitializedBoundNames, expr, FIXME: iterationKind ), https://tc39.es/ecma262/#sec-runtime-semantics-forinofheadevaluation
    // FIXME: Implement iterator records and use them in this function
    private AbruptOr<Object> ForInOfHeadEvaluation(VM vm)
    {
        // 1. Let oldEnv be the running execution context's LexicalEnvironment.
        var currentExecutionContext = (vm.CurrentExecutionContext as ScriptExecutionContext)!;
        var oldEnv = currentExecutionContext.LexicalEnvironment;

        // FIXME: 2. If uninitializedBoundNames is not empty, then
        // FIXME: a. Assert: uninitializedBoundNames has no duplicate entries.
        // FIXME: b. Let newEnv be NewDeclarativeEnvironment(oldEnv).
        // FIXME: c. For each String name of uninitializedBoundNames, do
        // FIXME: i. Perform ! newEnv.CreateMutableBinding(name, false).
        // FIXME: d. Set the running execution context's LexicalEnvironment to newEnv.

        // 3. Let exprRef be Completion(Evaluation of expr).
        var exprRef = Expression.Evaluate(vm);

        // 4. Set the running execution context's LexicalEnvironment to oldEnv.
        currentExecutionContext.LexicalEnvironment = oldEnv;

        // 5. Let exprValue be ? GetValue(? exprRef).
        if (exprRef.IsAbruptCompletion()) return exprRef;

        var exprValueResult = exprRef.Value.GetValue(vm);
        if (exprValueResult.IsAbruptCompletion()) return exprValueResult;
        var exprValue = exprValueResult.Value;

        // FIXME: 6. If iterationKind is ENUMERATE, then

        // a. If exprValue is either undefined or null, then
        if (exprValue.IsUndefined() || exprValue.IsNull())
        {
            // i. Return Completion Record { [[Type]]: BREAK, [[Value]]: EMPTY, [[Target]]: EMPTY }.
            return Completion.BreakCompletion(Empty.The, "");
        }

        // FIXME: b. Let obj be ! ToObject(exprValue).
        // FIXME: c. Let iterator be EnumerateObjectProperties(obj).
        // FIXME: d. Let nextMethod be ! GetV(iterator, "next").
        // FIXME: e. Return the Iterator Record { [[Iterator]]: iterator, [[NextMethod]]: nextMethod, [[Done]]: false }.
        // FIXME: 7. Else,
        // FIXME: a. Assert: iterationKind is either ITERATE or ASYNC-ITERATE.
        // FIXME: b. If iterationKind is ASYNC-ITERATE, let iteratorKind be ASYNC.
        // FIXME: c. Else, let iteratorKind be SYNC.
        // FIXME: d. Return ? GetIterator(exprValue, iteratorKind).
        return MUST(exprValue.ToObject(vm));
    }

    // 14.7.5.7 ForIn/OfBodyEvaluation ( lhs, stmt, iteratorRecord, iterationKind, lhsKind, labelSet [ , iteratorKind ] ), https://tc39.es/ecma262/#sec-runtime-semantics-forin-div-ofbodyevaluation-lhs-stmt-iterator-lhskind-labelset
    private Completion ForInOfBodyEvaluation(VM vm, Object keyResult)
    {
        // FIXME: Go through prototype data properties, if we need it before we implement iterators
        // FIXME: Other steps omitted for breavity, we mimic the enumerate iterator kind by only going through enumerable properties

        // 3. Let V be undefined.
        var V = (Value)Undefined.The;

        // NOTE: This is a hack for making we don't have a modification while enumerating exception.
        // This some-what emulates normal JS behaviour as defining a new enumerable property in the for-in loop does not get enumerated over
        var dataProperties = keyResult.DataProperties.ToDictionary(entry => entry.Key, entry => entry.Value);

        Completion status;
        Completion result = Completion.NormalCompletion(Empty.The);
        foreach (var (name, property) in dataProperties)
        {
            // NOTE: Skips non-enumerable properties as a enumerable iterator would do
            if (!property.Attributes.Enumerable) continue;

            // 1. Let lhsRef be Completion(Evaluation of lhs). (It may be evaluated repeatedly.)
            var lhsRef = Identifier.Evaluate(vm);

            // 2. If lhsRef is an abrupt completion, then
            if (lhsRef.IsAbruptCompletion())
            {
                // a. Let status be lhsRef.
                status = lhsRef;
            }
            // 3. Else,
            else
            {
                // a. Let status be Completion(PutValue(lhsRef.[[Value]], nextValue)).
                status = lhsRef.Value.PutValue(vm, name);
            }

            // i. If status is an abrupt completion, then
            if (status.IsAbruptCompletion())
            {
                // 1. Return ? status.
                return status;
            }

            // j. Let result be Completion(Evaluation of stmt).
            result = IterationStatement.Evaluate(vm);

            // m. If result.[[Value]] is not EMPTY, set V to result.[[Value]].
            if (!result.Value.IsEmpty()) V = result.Value;
        }

        // 1. Return ? UpdateEmpty(result, V).
        result!.UpdateEmpty(V);
        return result;
    }

    // 14.1.1 Runtime Semantics: Evaluation, https://tc39.es/ecma262/#sec-statement-semantics-runtime-semantics-evaluation
    public override Completion Evaluate(VM vm)
    {
        // FIXME: 1. Let newLabelSet be a new empty List.

        // 2. Return ? LabelledEvaluation of this BreakableStatement with argument newLabelSet.
        return LabelledStatement.LabelledBreakableEvaluation(vm, this);
    }

    public Identifier Identifier { get; }
    public IExpression Expression { get; }
    public INode IterationStatement { get; }
}
