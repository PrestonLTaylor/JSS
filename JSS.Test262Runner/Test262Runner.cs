using JSS.Common;
using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.Test262Runner;

enum TestResultType
{
    SUCCESS,
    METADATA_PARSING_FAILURE,
    HARNESS_EXECUTION_FAILURE,
    PARSING_FAILURE,
    CRASH_FAILURE,
    FAILURE,
}


// FIXME: Accept a filter from the command line to filter the tests being executed
/// <summary>
/// Our implementation for a runner for the test-262 test suite using the "test262" git repository that should be in the same directory.
/// </summary>
internal sealed class Test262Runner
{
    public Test262Runner()
    {
        _harnessNameToContent = ReadHarnessFiles();
    }

    /// <summary>
    /// Reads the harness files required for running tests from the test-262 repository.
    /// </summary>
    /// <returns>A map of the harness file name to the harness' file content.</returns>
    static private Dictionary<string, string> ReadHarnessFiles()
    {
        const string HARNESS_DIRECTORY = "./test262/harness";
        const string HARNESS_JAVASCRIPT_FILTER = "*.js";

        var harnessNameToContent = new Dictionary<string, string>();
        var harnessFiles = Directory.EnumerateFiles(HARNESS_DIRECTORY, HARNESS_JAVASCRIPT_FILTER, SearchOption.AllDirectories);
        foreach (var harnessFile in harnessFiles)
        {
            var fileName = Path.GetFileName(harnessFile);
            var fileContent = File.ReadAllText(harnessFile);
            harnessNameToContent.Add(fileName, fileContent);
        }

        return harnessNameToContent;
    }

    // FIXME: Use OneOf instead of throwing exceptions to indicate failures
    record TestResult(TestResultType Type, string FailureReason = "");

    /// <summary>
    /// Starts the <see cref="Test262Runner"/> instance.
    /// </summary>
    public void StartRunner()
    {
        const string TEST_DIRECTORY = "./test262/test";
        const string TEST_FILTER = "*.js";

        var testResults = CreateTestResultsDictionary();
        var testFiles = Directory.EnumerateFiles(TEST_DIRECTORY, TEST_FILTER, SearchOption.AllDirectories);
        foreach (var testFile in testFiles)
        {
            var testCase = File.ReadAllText(testFile);
            var testResult = ExecuteTestCase(testCase);

            ++testResults[testResult.Type];

            LogTestResult(testFile, testResult);
        }

        LogTestRunStatistics(testFiles.Count(), testResults);
    }

    /// <summary>
    /// Creates a zeroed dictionary for test results.
    /// </summary>
    static private Dictionary<TestResultType, int> CreateTestResultsDictionary()
    {
        return new()
        {
            { TestResultType.SUCCESS, 0 },
            { TestResultType.METADATA_PARSING_FAILURE, 0 },
            { TestResultType.HARNESS_EXECUTION_FAILURE, 0 },
            { TestResultType.PARSING_FAILURE, 0 },
            { TestResultType.CRASH_FAILURE, 0 },
            { TestResultType.FAILURE, 0 },
        };
    }

    /// <summary>
    /// Executes a test case and reports the result of executing the provided test case.
    /// </summary>
    /// <param name="testCase">The test case code to be executed as a string.</param>
    /// <returns>The result of executing the test case.</returns>
    private TestResult ExecuteTestCase(string testCase)
    {
        Test262Metadata? testCaseMetadata = null;
        try
        {
            testCaseMetadata = Test262Metadata.Create(testCase);
            var testCaseVm = CreateTestCaseVM(testCaseMetadata);
            var testCaseScript = ParseAsGlobalCode(testCaseVm, testCase);
            var testCompletion = testCaseScript.ScriptEvaluation();

            return CreateTestCaseResult(testCaseVm, testCaseMetadata, testCompletion);
        }
        catch (MetadataParsingFailureException ex)
        {
            return new(TestResultType.METADATA_PARSING_FAILURE, ex.Message);
        }
        catch (HarnessExecutionFailureException ex)
        {
            return new(TestResultType.HARNESS_EXECUTION_FAILURE, ex.Message);
        }
        catch (SyntaxErrorException ex)
        {
            if (testCaseMetadata!.ExpectedTestResultType == TestResultType.PARSING_FAILURE
                && testCaseMetadata!.IsExpectedNegativeErrorType("SyntaxError")) return new(TestResultType.SUCCESS);
            return new(TestResultType.PARSING_FAILURE, ex.Message);
        }
        // NOTE: If we catch an exception that we don't expect, that means that process would crash, if not caught.
        catch (Exception ex)
        {
            return new(TestResultType.CRASH_FAILURE, ex.Message);
        }
    }

    /// <summary>
    /// Creates an isolated VM for running a single test-262 test file.
    /// </summary>
    /// <returns>An isolated VM with its own dedicated ECMAScript realm as specified in INTERPRETING.md</returns>
    private VM CreateTestCaseVM(Test262Metadata metadata)
    {
        var completion = Realm.InitializeHostDefinedRealm(out VM testCaseVm);
        if (completion.IsAbruptCompletion())
        {
            throw new HarnessExecutionFailureException(testCaseVm, completion);
        }

        // raw: The test source code must not be modified in any way, files from the harness/directory must not be evaluated,
        // FIXME: and the test must be executed just once (in non-strict mode, only).
        if (!metadata.HasFlag("raw"))
        {
            ExecuteRequiredHarnessFiles(testCaseVm, metadata);
        }

        return testCaseVm;
    }

    /// <summary>
    /// Executes the required harness files needed to run tests on the provided <paramref name="vm"/>.
    /// </summary>
    /// <param name="vm">The VM to execute the required harness files on.</param>
    private void ExecuteRequiredHarnessFiles(VM vm, Test262Metadata metadata)
    {
        try
        {
            // includes: One or more files whose content must be evaluated in the test realm's global scope prior to test execution.
            string[] requiredHarnessFiles = [..REQUIRED_HARNESS_FILE_NAMES, ..metadata.Includes];
            foreach (var requiredHarnessFile in requiredHarnessFiles)
            {
                // NOTE: We throw the base Exception to signify that this would be a crash failure if executed.
                // FIXME: Test cases using tcoHelper will check to prove TCO works, however, we have no TCO functionality and will cause the runner to crash with a stack overflow exception.
                if (requiredHarnessFile == "tcoHelper.js") throw new Exception("tcoHelper.js was included, this test case will probably cause a stack overflow exception.");

                var harnessScriptString = _harnessNameToContent[requiredHarnessFile];
                var harnessScript = ParseAsGlobalCode(vm, harnessScriptString);
                var harnessCompletion = harnessScript.ScriptEvaluation();
                if (harnessCompletion.IsAbruptCompletion())
                {
                    throw new HarnessExecutionFailureException(vm, harnessCompletion);
                }
            }
        }
        // NOTE: We want to rethrow syntax errors as harness execution failures
        // FIXME: If we want to print stack traces in the future, we need to preverse the original exception
        catch (SyntaxErrorException ex)
        {
            throw new HarnessExecutionFailureException(ex.Message);
        }
    }

    // FIXME: Currently we can only parse scripts as global code, but we need a mechanism for parsing scripts as module code.
    /// <summary>
    /// Parses the <paramref name="scriptString"/> as global code and returns the executable <see cref="Script"/>.
    /// </summary>
    /// <param name="vm">The <see cref="VM"/> to use for the parsed script.</param>
    /// <param name="scriptString">The script to parse as a string.</param>
    /// <returns>A <see cref="Script"/> that contains the AST built from the provided <paramref name="scriptString"/>.</returns>
    /// <exception cref="SyntaxErrorException">Thrown if the <paramref name="scriptString"/> is not a valid JavaScript file or not parseable by our parser.</exception>
    static private Script ParseAsGlobalCode(VM vm, string scriptString)
    {
        var parser = new Parser(scriptString);
        return parser.Parse(vm);
    }

    /// <summary>
    /// Creates a test case result based on the metadata of the test and the completion of the test.
    /// </summary>
    /// <param name="vm">The VM that executed the test case.</param>
    /// <param name="metadata">The metadata of the test case.</param>
    /// <param name="completion">The completion from executing the test case.</param>
    /// <returns>A test case based on the metadata and completion of the test.</returns>
    private TestResult CreateTestCaseResult(VM vm, Test262Metadata metadata, Completion completion)
    {
        if (metadata.IsNegativeTestCase)
        {
            if (completion.IsNormalCompletion()) return new(TestResultType.FAILURE, "Negative test case was executed successfully.");

            var errorType = Print.GetErrorNameFromValue(completion.Value);
            if (metadata.ExpectedTestResultType != TestResultType.FAILURE || !metadata.IsExpectedNegativeErrorType(errorType))
            {
                return new TestResult(metadata.ExpectedTestResultType,
                    $"Negative test case expected an error type of \"{metadata.NegativeTestCaseType}\" but got a completion of {Print.CompletionToString(vm, completion)}.");
            }

            return new TestResult(TestResultType.SUCCESS);
        }
        else
        {
            if (completion.IsAbruptCompletion()) return new(TestResultType.FAILURE, Print.CompletionToString(vm, completion));

            return new(TestResultType.SUCCESS);
        }
    }

    /// <summary>
    /// Logs the result of a test case, currently, we only log to the console.
    /// </summary>
    /// <param name="testPath">The un-pretty file path to the executed test.</param>
    /// <param name="testResult">The result of the executed test.</param>
    static private void LogTestResult(string testPath, TestResult testResult)
    {
        var prettyPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), testPath);
        var emoji = TEST_RESULT_TYPE_TO_EMOJI[testResult.Type];
        if (testResult.Type == TestResultType.SUCCESS)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red ;
        }

        Console.WriteLine($"{prettyPath}: {emoji}");
        if (testResult.FailureReason != "") Console.WriteLine(testResult.FailureReason);
        Console.ResetColor();
    }

    static private void LogTestRunStatistics(int testCount, Dictionary<TestResultType, int> testResults)
    {
        var testSuccessCount = testResults[TestResultType.SUCCESS];
        var testSuccessPercent = testSuccessCount / (double)testCount * 100;
        Console.WriteLine($"{testCount} test cases executed.");
        Console.WriteLine($"{testSuccessPercent}% of tests passed.");

        foreach (var (result, count) in testResults)
        {
            var emoji = TEST_RESULT_TYPE_TO_EMOJI[result];
            Console.Write($"{emoji}: {count}\t");
        }
    }

    // https://github.com/tc39/test262/blob/main/INTERPRETING.md states that assert.js and sta.js must be evaluted before each test file is executed.
    static private readonly string[] REQUIRED_HARNESS_FILE_NAMES = ["assert.js", "sta.js"];

    static private readonly Dictionary<TestResultType, string> TEST_RESULT_TYPE_TO_EMOJI = new()
    {
        { TestResultType.SUCCESS, "✅" },
        { TestResultType.METADATA_PARSING_FAILURE, "📝" },
        { TestResultType.HARNESS_EXECUTION_FAILURE, "⚙️" },
        { TestResultType.PARSING_FAILURE, "✍️" },
        { TestResultType.CRASH_FAILURE, "💥" },
        { TestResultType.FAILURE, "❌" },
    };

    private readonly Dictionary<string, string> _harnessNameToContent;
}
