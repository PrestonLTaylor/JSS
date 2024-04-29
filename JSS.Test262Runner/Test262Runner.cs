using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.Test262Runner;

enum TestResult
{
    SUCCESS,
    HARNESS_EXECUTION_FAILURE,
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

            ++testResults[testResult];

            LogTestResult(testFile, testResult);
        }

        LogTestRunStatistics(testFiles.Count(), testResults);
    }

    /// <summary>
    /// Creates a zeroed dictionary for test results.
    /// </summary>
    static private Dictionary<TestResult, int> CreateTestResultsDictionary()
    {
        return new()
        {
            { TestResult.SUCCESS, 0 },
            { TestResult.FAILURE, 0 },
            { TestResult.HARNESS_EXECUTION_FAILURE, 0 }
        };
    }

    /// <summary>
    /// Executes a test case and reports the result of executing the provided test case.
    /// </summary>
    /// <param name="testCase">The test case code to be executed as a string.</param>
    /// <returns>The result of executing the test case.</returns>
    private TestResult ExecuteTestCase(string testCase)
    {
        try
        {
            var testCaseVm = CreateTestCaseVM();
            var testCaseScript = ParseAsGlobalCode(testCaseVm, testCase);
            var testCompletion = testCaseScript.ScriptEvaluation();

            if (testCompletion.IsAbruptCompletion()) return TestResult.FAILURE;

            return TestResult.SUCCESS;
        }
        catch (HarnessExecutionFailureException)
        {
            return TestResult.HARNESS_EXECUTION_FAILURE;
        }
        catch
        {
            return TestResult.FAILURE;
        }
    }

    /// <summary>
    /// Creates an isolated VM for running a single test-262 test file.
    /// </summary>
    /// <returns>An isolated VM with its own dedicated ECMAScript realm as specified in INTERPRETING.md</returns>
    private VM CreateTestCaseVM()
    {
        var completion = Realm.InitializeHostDefinedRealm(out VM testCaseVm);
        if (completion.IsAbruptCompletion())
        {
            throw new HarnessExecutionFailureException(testCaseVm, completion);
        }

        ExecuteRequiredHarnessFiles(testCaseVm);

        return testCaseVm;
    }

    /// <summary>
    /// Executes the required harness files needed to run tests on the provided <paramref name="vm"/>.
    /// </summary>
    /// <param name="vm">The VM to execute the required harness files on.</param>
    private void ExecuteRequiredHarnessFiles(VM vm)
    {
        try
        {
            foreach (var requiredHarnessFile in REQUIRED_HARNESS_FILE_NAMES)
            {
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
    /// Logs the result of a test case, currently, we only log to the console.
    /// </summary>
    /// <param name="testPath">The un-pretty file path to the executed test.</param>
    /// <param name="testResult">The result of the executed test.</param>
    static private void LogTestResult(string testPath, TestResult testResult)
    {
        var prettyPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), testPath);
        var emoji = TEST_RESULT_TO_EMOJIS[testResult];
        if (testResult == TestResult.SUCCESS)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red ;
        }

        Console.WriteLine($"{prettyPath}: {emoji}");
        Console.ResetColor();
    }

    static private void LogTestRunStatistics(int testCount, Dictionary<TestResult, int> testResults)
    {
        var testSuccessCount = testResults[TestResult.SUCCESS];
        var testSuccessPercent = testSuccessCount / (double)testCount * 100;
        Console.WriteLine($"{testCount} test cases executed.");
        Console.WriteLine($"{testSuccessPercent}% of tests passed.");

        foreach (var (result, count) in testResults)
        {
            var emoji = TEST_RESULT_TO_EMOJIS[result];
            Console.Write($"{emoji}: {count}\t");
        }
    }

    // FIXME: Implement a YAML parser and only execute harness files needed for each test
    // https://github.com/tc39/test262/blob/main/INTERPRETING.md states that assert.js and sta.js must be evaluted before each test file is executed.
    static private readonly string[] REQUIRED_HARNESS_FILE_NAMES = ["assert.js", "sta.js", "propertyHelper.js"];

    static private readonly Dictionary<TestResult, string> TEST_RESULT_TO_EMOJIS = new()
    {
        { TestResult.SUCCESS, "✅" },
        { TestResult.HARNESS_EXECUTION_FAILURE, "⚙️" },
        { TestResult.FAILURE, "❌" },
    };

    private readonly Dictionary<string, string> _harnessNameToContent;
}
