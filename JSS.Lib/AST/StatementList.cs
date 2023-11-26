using JSS.Lib.Execution;

namespace JSS.Lib.AST;

// StatementList, https://tc39.es/ecma262/#prod-StatementList
internal sealed class StatementList : INode
{
    public StatementList(List<INode> statements)
    {
        Statements = statements;
    }

    // 14.2.2 Runtime Semantics: Evaluation, StatementList, https://tc39.es/ecma262/#sec-block-runtime-semantics-evaluation
    override public Completion Evaluate(VM vm)
    {
        // 1. Let sl be ? Evaluation of StatementList.
        Completion completion = Completion.NormalCompletion(vm.Empty);
        foreach (var statement in Statements)
        {
            // 2. Let s be Completion(Evaluation of StatementListItem).
            var s = statement.Evaluate(vm);

            // 3. Return ? UpdateEmpty(s, sl).
            s.UpdateEmpty(completion.Value);
            completion = s;
            if (completion.IsAbruptCompletion())
            {
                return completion;
            }
        }

        return completion;
    }

    public IReadOnlyList<INode> Statements { get; }
}
