namespace JSS.Test262Runner;

internal record TestResult(TestResultType Type, string TestPath, string FailureReason = "")
{
    static public readonly IReadOnlyDictionary<TestResultType, string> TEST_RESULT_TYPE_TO_EMOJI = new Dictionary<TestResultType, string>()
    {
        { TestResultType.SUCCESS, "✅" },
        { TestResultType.METADATA_PARSING_FAILURE, "📝" },
        { TestResultType.HARNESS_EXECUTION_FAILURE, "⚙️" },
        { TestResultType.PARSING_FAILURE, "✍️" },
        { TestResultType.CRASH_FAILURE, "💥" },
        { TestResultType.FAILURE, "❌" },
    };
}
