﻿using JSS.Lib;
using JSS.Lib.Execution;

namespace JSS.Test262Runner;

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
        var testCaseVm = CreateTestCaseVM();
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
            // FIXME: Replace InvalidOperationException with specific exceptions for each step of running a test.
            throw new InvalidOperationException("Unable to initialize a host defined realm for a test case.");
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
        foreach (var requiredHarnessFile in REQUIRED_HARNESS_FILE_NAMES)
        {
            var harnessScriptString = _harnessNameToContent[requiredHarnessFile];
            var harnessScript = ParseAsGlobalCode(vm, harnessScriptString);
            var harnessCompletion = harnessScript.ScriptEvaluation();
            if (harnessCompletion.IsAbruptCompletion())
            {
                throw new InvalidOperationException($"The harness file {requiredHarnessFile} returned an abrupt completion.");
            }
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

    // FIXME: Implement a YAML parser and only execute harness files needed for each test
    // https://github.com/tc39/test262/blob/main/INTERPRETING.md states that assert.js and sta.js must be evaluted before each test file is executed.
    static private readonly string[] REQUIRED_HARNESS_FILE_NAMES = ["assert.js", "sta.js", "propertyHelper.js"];

    private readonly Dictionary<string, string> _harnessNameToContent;
}