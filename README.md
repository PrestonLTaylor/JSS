# JSS (JavaScript Sharp)

[![Build and Test JSS](https://github.com/PrestonLTaylor/JSS/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/PrestonLTaylor/JSS/actions/workflows/build-and-test.yml)

JavaScript Sharp is an in-progress C# JavaScript parser, runtime engine and REPL.

## Table of contents

- [Sceenshots](#screenshots)
- [Usage](#usage)
  - [CLI](#cli)
  - [Library](#library)
- [Building](#building)
- [Testing](#testing)
  - [Unit Tests](#unit-tests)
  - [Test262 Runner](#test262-runner)
- [License](#license)

# Screenshots

![Screenshot of the JSS CLI](https://i.imgur.com/AjCbgbA.png)

![Screenshot of the JSS Test262 runner](https://i.imgur.com/OOagMem.png)

# Usage

## CLI

The CLI is used to use the JSS JavaScript engine from your terminal.

There are two modes for the CLI; the REPL and the file executor.

To use the repl you execute the JSS executable with no extra command line arguments:

```
JSS.CLI.exe
```

To use the file execute you execute the JSS repl with the the -s/--script argument with the path to the file you wish to execute:

```
JSS.CLI.exe -s ./path/to/script.js
```

## Library

JSS also exposes a library that can be used to execute JavaScript code using the JSS engine.

To execute a script using the JSS engine inside of C#:

```c#
var initializeResult = JSS.Lib.Realm.InitializeHostDefinedRealm(out VM vm); // Initializes the realm to execute JavaScript inside of
var parser = new JSS.Lib.Parser(SCRIPT_CODE); // Creates a new parser for the script provided
var script = parser.Parse(vm); // Returns a script object from the parsed script using the provided vm
var scriptResult = script.ScriptEvaluation(CancellationToken.None); // Executes the script
```

The VM created by the `InitializeHostDefinedRealm` contains all the variables and functions defined when executing a script. This VM can be used multiple times by different scripts.

The result returned from `ScriptEvalaution` is a "Completion". A completion holds a javascript value and the type of completion it is.

`scriptResult.Value` would be the JavaScript value from executing the script.

# Building

Building JSS is simple, you can do it within Visual Studio or by simply executing in the root of the solution:

```
dotnet build (-c Release or Debug)
```

The executables will be located in the bin folders for each project (e.g. JSS/bin/Debug/net7.0/JSS.CLI.exe).

# Testing

## Unit Tests

Our unit test suite comprises of C# unit tests that mainly focus on the the lexing, parsing etc. rather than the runtime. There are a few runtime tests, however, we should ideally move them to their own runtime test suite and use JavaScript files.

To run these tests simply execute in the root of the solution:

```
dotnet test
```

## Test262 Runner

Our test262 runner is for running the [Official ECMAScript Conformance Test Suite](https://github.com/tc39/test262). This gives us access to a comprehensive set of JavaScript test files to test JSS with.

We also have a [CI job](https://github.com/PrestonLTaylor/JSS/actions/workflows/test-262-runner.yml) for running these tests.

Our test262 runner has two modes, the runner itself and a test run "differ"

The runner executes tests from the test262 test suite and prints its output to the console and to a file in the same directory.

To execute the runner:

```
JSS.Test262Runner.exe
```

The runner can also filter the tests to execute based on the file path by using the -f flag.

For example, if you wanted to only run the [tests for undefined](https://github.com/tc39/test262/tree/main/test/built-ins/undefined) where the '\*' represents a wildcard:

```
JSS.Test262Runner.exe -f "./built-ins/undefined/*.js"
```

The differ performs a visual difference between two test run file outputs.

To execute the differ:

```
JSS.Test262Runner.exe diff --from "/path/to/from-test-run.txt" --to "/path/to/to-test-run.txt"
```

This will output all the tests that changed between the two test runs. For example, if a test passed and now fails that will show up on the differ.

# License

This project is licensed under the [MIT License](https://github.com/PrestonLTaylor/JSS/blob/master/LICENSE).
