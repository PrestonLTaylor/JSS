﻿using JSS.Common;
using JSS.Lib;
using JSS.Lib.Execution;
using System.Text.Json.Nodes;

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
    public Test262Runner(RunnerOptions options)
    {
        _options = options;
        _harnessNameToContent = ReadHarnessFiles();
    }

    /// <summary>
    /// Reads the harness files required for running tests from the test-262 repository.
    /// </summary>
    /// <returns>A map of the harness file name to the harness' file content.</returns>
    static private Dictionary<string, string> ReadHarnessFiles()
    {
        const string HARNESS_DIRECTORY = ".\\test262\\harness";
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


    /// <summary>
    /// Starts the <see cref="Test262Runner"/> instance.
    /// </summary>
    public void StartRunner()
    {
        var testFiles = GetTestCasePathsToExecute();
        if (testFiles is null) return;

        var testResults = CreateTestResultsDictionary();
        var testCounter = 0;
        var testCount = testFiles.Count();
        foreach (var testFile in testFiles)
        {
            var testPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), testFile);
            var testResult = ExecuteTestCase(testPath);

            testResults[testResult.Type].Add(testResult);
            ++testCounter;

            if (_options.Quiet)
            {
                QuietlyLogTestProgress(testCount, testCounter);
            }
            else
            {
                LogTestResult(testFile, testResult);
            }
        }

        LogTestRunStatistics(testCount, testResults);
    }

    /// <summary>
    /// Gets the paths of test case files to execute or null if command line filter is incorrect.
    /// </summary>
    /// <returns>An enumerable containing the paths of test cases to execute or null if command line filter is incorrect.</returns>
    private IEnumerable<string>? GetTestCasePathsToExecute()
    {
        // Modules, Test262 includes tests for ECMAScript 2015 module code, denoted by the "module" metadata flag.
        // Files bearing a name which includes the sequence _FIXTURE MUST NOT be interpreted as standalone tests; they are intended to be referenced by test files.
        const string TEST_FIXTURE = "_FIXTURE";
        const string TEST_DIRECTORY = ".\\test262\\test";

        var filter = _options.Filter;
        if (!_options.Filter.EndsWith(".js")) filter += ".js";

        try
        {
            return Directory.EnumerateFiles(TEST_DIRECTORY, filter, SearchOption.AllDirectories)
                .Where(x => !x.Contains(TEST_FIXTURE));
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The filter {filter} is invalid.");
            Console.ResetColor();
            return null;
        }
    }

    /// <summary>
    /// Creates an empty dictionary for test results.
    /// </summary>
    static private Dictionary<TestResultType, List<TestResult>> CreateTestResultsDictionary()
    {
        return new()
        {
            { TestResultType.SUCCESS, [] },
            { TestResultType.METADATA_PARSING_FAILURE, [] },
            { TestResultType.HARNESS_EXECUTION_FAILURE, [] },
            { TestResultType.PARSING_FAILURE, [] },
            { TestResultType.CRASH_FAILURE, [] },
            { TestResultType.FAILURE, [] },
        };
    }

    /// <summary>
    /// Executes a test case and reports the result of executing the provided test case.
    /// </summary>
    /// <param name="testCase">The test case code to be executed as a string.</param>
    /// <returns>The result of executing the test case.</returns>
    private TestResult ExecuteTestCase(string testPath)
    {
        var testCase = File.ReadAllText(testPath);

        Test262Metadata? testCaseMetadata = null;
        try
        {
            testCaseMetadata = Test262Metadata.Create(testCase);

            TestResult? result = null;
            if (ShouldRunInNonStrictMode(testCaseMetadata))
            {
                result = ExecuteTestCaseString(testCaseMetadata, testPath, testCase);
                if (result.Type != TestResultType.SUCCESS) return result;
            }
            if (ShouldRunInStrictMode(testCaseMetadata))
            {
                // To run in strict mode, the test contents must be modified prior to execution a "use strict" directive must be inserted
                // as the initial character sequence of the file, followed by a semicolon (;) and newline character (\n)
                result = ExecuteTestCaseString(testCaseMetadata, testPath, "'use strict';\n" + testCase);
            }

            return result!;

        }
        catch (MetadataParsingFailureException ex)
        {
            return new(TestResultType.METADATA_PARSING_FAILURE, testPath, ex.Message);
        }
        catch (HarnessExecutionFailureException ex)
        {
            return new(TestResultType.HARNESS_EXECUTION_FAILURE, testPath, ex.Message);
        }
        catch (SyntaxErrorException ex)
        {
            if (testCaseMetadata!.ExpectedTestResultType == TestResultType.PARSING_FAILURE
                && testCaseMetadata!.IsExpectedNegativeErrorType("SyntaxError")) return new(TestResultType.SUCCESS, testPath);
            return new(TestResultType.PARSING_FAILURE, testPath, ex.Message);
        }
        // NOTE: If we catch an exception that we don't expect, that means that process would crash, if not caught.
        catch (Exception ex)
        {
            return new(TestResultType.CRASH_FAILURE, testPath, ex.Message);
        }
    }

    /// <summary>
    /// Determines if a test case should be ran in non-strict mode.
    /// If a test case has the "raw" flag set or does not have the "onlyStrict" flag set then it should be ran in non-strict mode.
    /// </summary>
    /// <param name="metadata">The metadata associated with a test case.</param>
    /// <returns><see cref="true"/> if the test case should be ran in non-strict mode, otherwise, <see cref="false"/>.</returns>
    static private bool ShouldRunInNonStrictMode(Test262Metadata metadata)
    {
        return metadata.HasFlag("raw") || !metadata.HasFlag("onlyStrict");
    }

    /// <summary>
    /// Determines if a test case should be ran in strict mode.
    /// If a test case has does not have the "raw" or "noStrict" flag set then it should be ran in strict mode.
    /// </summary>
    /// <param name="metadata">The metadata associated with a test case.</param>
    /// <returns><see cref="true"/> if the test case should be ran in strict mode, otherwise, <see cref="false"/>.</returns>
    static private bool ShouldRunInStrictMode(Test262Metadata metadata)
    {
        return !metadata.HasFlag("raw") && !metadata.HasFlag("noStrict");
    }

    /// <summary>
    /// Executes a test case string and reports the result of executing the provided test case.
    /// </summary>
    /// <param name="metadata">The metadata associated with the test case.</param>
    /// <param name="testCase">The path of the test case.</param>
    /// <param name="testCase">The test case string to execute.</param>
    /// <returns>The result of executing the test case string.</returns>
    private TestResult ExecuteTestCaseString(Test262Metadata metadata, string testPath, string testCase)
    {
        var testCaseVm = CreateTestCaseVM(metadata);
        var testCaseScript = ParseAsGlobalCode(testCaseVm, testCase);
        var testCompletion = testCaseScript.ScriptEvaluation();

        return CreateTestCaseResult(testPath, testCaseVm, metadata, testCompletion);
    }

    /// <summary>
    /// Creates an isolated VM for running a single test-262 test file.
    /// </summary>
    /// <returns>An isolated VM with its own dedicated ECMAScript realm as specified in INTERPRETING.md</returns>
    private VM CreateTestCaseVM(Test262Metadata metadata)
    {
        var initializeCompletion = Realm.InitializeHostDefinedRealm(out VM testCaseVm);
        if (initializeCompletion.IsAbruptCompletion())
        {
            throw new HarnessExecutionFailureException(testCaseVm, initializeCompletion);
        }

        var test262DefiningCompletion = Realm.CreateTest262HostDefinedFunctions(testCaseVm);
        if (test262DefiningCompletion.IsAbruptCompletion())
        {
            throw new HarnessExecutionFailureException(testCaseVm, test262DefiningCompletion);
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
                //if (requiredHarnessFile == "tcoHelper.js") throw new Exception("tcoHelper.js was included, this test case will probably cause a stack overflow exception.");

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
    /// <param name="testPath">The file path of the test case.</param>
    /// <param name="vm">The VM that executed the test case.</param>
    /// <param name="metadata">The metadata of the test case.</param>
    /// <param name="completion">The completion from executing the test case.</param>
    /// <returns>A test case based on the metadata and completion of the test.</returns>
    static private TestResult CreateTestCaseResult(string testPath, VM vm, Test262Metadata metadata, Completion completion)
    {
        if (metadata.IsNegativeTestCase)
        {
            if (completion.IsNormalCompletion()) return new(TestResultType.FAILURE, testPath, "Negative test case was executed successfully.");

            var errorType = Print.GetErrorNameFromValue(completion.Value);
            if (metadata.ExpectedTestResultType != TestResultType.FAILURE || !metadata.IsExpectedNegativeErrorType(errorType))
            {
                return new TestResult(metadata.ExpectedTestResultType, testPath,
                    $"Negative test case expected an error type of \"{metadata.NegativeTestCaseType}\" but got a completion of {Print.CompletionToString(vm, completion)}.");
            }

            return new TestResult(TestResultType.SUCCESS, testPath);
        }
        else
        {
            if (completion.IsAbruptCompletion()) return new(TestResultType.FAILURE, testPath, Print.CompletionToString(vm, completion));

            return new(TestResultType.SUCCESS, testPath);
        }
    }

    /// <summary>
    /// Prevents spamming console output by only printing an update message every 100 tests.
    /// </summary>
    /// <param name="testCount">The total number of test-262 tests to execute.</param>
    /// <param name="testsCompleted">The number of completed test-262 tests</param>
    static private void QuietlyLogTestProgress(int testCount, int testsCompleted)
    {
        const int TESTS_BETWEEN_QUIET_PRINT = 100;
        if (testsCompleted % TESTS_BETWEEN_QUIET_PRINT == 0)
        {
            var testSuccessPercent = testsCompleted / (double)testCount * 100;
            Console.WriteLine($"{testSuccessPercent:0.##}% ({testsCompleted}/{testCount}) of tests completed.");
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
        var emoji = TestResult.TEST_RESULT_TYPE_TO_EMOJI[testResult.Type];
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

    /// <summary>
    /// Logs the statistics of the current test run.
    /// </summary>
    /// <param name="testCount">The total number of tests executed.</param>
    /// <param name="testResults">The result of every test executed.</param>
    static private void LogTestRunStatistics(int testCount, Dictionary<TestResultType, List<TestResult>> testResults)
    {
        LogTestRunStatisticsToConsole(testCount, testResults);
        LogTestRunStatisticsToAJsonFile(testResults);
    }

    static private void LogTestRunStatisticsToConsole(int testCount, Dictionary<TestResultType, List<TestResult>> testResults)
    {
        LogPathGroupedStatisticsToConsole(testResults);

        var testSuccesses = testResults[TestResultType.SUCCESS];
        var testSuccessPercent = testSuccesses.Count / (double)Math.Max(testCount, 1) * 100;
        Console.WriteLine($"{testCount} test cases executed.");
        Console.WriteLine($"{testSuccessPercent:0.##}% of tests passed.");

        foreach (var (result, results) in testResults)
        {
            var emoji = TestResult.TEST_RESULT_TYPE_TO_EMOJI[result];
            Console.Write($"{emoji}: {results.Count}    ");
        }
    }

    static private void LogPathGroupedStatisticsToConsole(Dictionary<TestResultType, List<TestResult>> testResults)
    {
        // FIXME: Might be more optimal to store the test results in the trie to begin with instead of a dictionary
        var rootPathTrie = new TestResultPathTrie();
        foreach (var (type, results) in testResults)
        {
            foreach (var result in results)
            {

                rootPathTrie.Add(result.TestPath, type);
            }
        }

        rootPathTrie.Visit((currentPath, currentResultCount) =>
        {
            // Removes the "/test262/test/ from being shown
            const int COUNT_TO_REMOVE_TEST_262_PATH = 15;
            if (currentPath.Length <= COUNT_TO_REMOVE_TEST_262_PATH) return;
            var prettyPath = currentPath[COUNT_TO_REMOVE_TEST_262_PATH..];

            Console.Write($"/{prettyPath}/ => ");
            foreach (var (type, count) in currentResultCount)
            {
                if (count == 0) continue;
                var emoji = TestResult.TEST_RESULT_TYPE_TO_EMOJI[type];
                Console.Write($"{emoji}: {count}    ");
            }
            Console.WriteLine();
        });
    }

    static private void LogTestRunStatisticsToAJsonFile(Dictionary<TestResultType, List<TestResult>> testResults)
    {
        JsonObject resultsAsJson = [];
        foreach (var (_, results) in testResults)
        {
            foreach (var result in results)
            {
                resultsAsJson.Add(result.TestPath, JsonValue.Create(result));
            }
        }

        var resultsJsonString = resultsAsJson.ToString();
        File.WriteAllText($"test-262-run-{DateTime.Now:yy-M-dd-HH-mm}.txt", resultsJsonString);
    }

    // https://github.com/tc39/test262/blob/main/INTERPRETING.md states that assert.js and sta.js must be evaluted before each test file is executed.
    static private readonly string[] REQUIRED_HARNESS_FILE_NAMES = ["assert.js", "sta.js"];


    private readonly RunnerOptions _options;
    private readonly Dictionary<string, string> _harnessNameToContent;
}
