using BenchmarkDotNet.Attributes;
using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.Benchmarks.ParserBenchmarks;

public class ExpressionExecution
{
    [GlobalSetup]
    public void SetupVM()
    {
        var completion = Realm.InitializeHostDefinedRealm(out VM vm);
        if (completion.IsAbruptCompletion())
        {
            throw new InvalidOperationException("Unable to initialize a host defined realm");
        }

        var parser = new Parser(Expression);
        _script = parser.Parse(vm);
    }

    [Benchmark]
    public Completion ParseExpression()
    {
        return _script.ScriptEvaluation();
    }

    [ParamsSource(nameof(Expressions))]
    public string Expression;
    public string[] Expressions { get; } =
    [
        "1 || 2",
        "1 || 2 && 3",
        "1 || 2 && 3 | 4 & 5 ** 6",
        "((((1 + 2) * 3) / 4) !== 5)",
        "((((1 + 2) * 3) / 4) !== 5) + ((((1 + 2) * 3) / 4) !== 5) + ((((1 + 2) * 3) / 4) !== 5) + ((((1 + 2) * 3) / 4) !== 5) + ((((1 + 2) * 3) / 4) !== 5)",
    ];

    private Script _script;
}
