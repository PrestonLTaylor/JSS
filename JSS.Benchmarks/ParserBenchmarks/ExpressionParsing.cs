using BenchmarkDotNet.Attributes;
using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.Benchmarks.ParserBenchmarks;

public class ExpressionParsing
{
    [GlobalSetup]
    public void SetupVM()
    {
        var completion = Realm.InitializeHostDefinedRealm(out _vm);
        if (completion.IsAbruptCompletion())
        {
            throw new InvalidOperationException("Unable to initialize a host defined realm");
        }

        _parser = new Parser(Expression);
    }

    [Benchmark]
    public Script ParseExpression()
    {
        return _parser.Parse(_vm);
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

    private VM _vm;
    private Parser _parser;
}
