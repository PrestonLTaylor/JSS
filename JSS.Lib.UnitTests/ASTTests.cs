using JSS.Lib.AST.Values;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;
using String = JSS.Lib.AST.Values.String;
using JSS.Lib.AST;

namespace JSS.Lib.UnitTests;

// NOTE: These aren't traditional unit tests, as the units of work are based on the spec instead of code.
// However, if we can trust our parser then they are close to traditional unit tests.
internal sealed class ASTTests
{
    // FIXME: In general we need more varied test cases for a lot of specific parts of the langauge
    static private readonly object[] astTestCases =
    {
        // Tests for Literals
        CreateBooleanLiteralTestCase(false),
        CreateBooleanLiteralTestCase(true),

        // FIXME: More numeric test cases when we can parse more numbers
        CreateNumericLiteralTestCase(0),
        CreateNumericLiteralTestCase(1),
        CreateNumericLiteralTestCase(123),
        CreateNumericLiteralTestCase(1234567890),

        // FIXME: More string test cases when we can parse more strings (e.g. escape sequences)
        CreateStringLiteralTestCase("this is a string value"),
        CreateStringLiteralTestCase("\"", '\''),
        CreateStringLiteralTestCase("'"),

        // Tests for string concatination
        CreateStringConcatinationTestCase(EscapeString("lhs"), EscapeString("rhs"), "lhsrhs"),
        CreateStringConcatinationTestCase(EscapeString(""), "1", "1"),
        CreateStringConcatinationTestCase("1", EscapeString(""), "1"),

        // Tests for addition
        CreateAdditionTestCase(1, 1, 2),
        CreateAdditionTestCase(1, 0, 1),

        // Tests for an empty statement
        new object[] { ";", Completion.NormalCompletion(new Undefined()) },

        // Tests for a block
        new object[] { "{ }", Completion.NormalCompletion(new Undefined()) },
        new object[] { "{ true }", Completion.NormalCompletion(new Boolean(true)) },
        new object[] { "{ 1 }", Completion.NormalCompletion(new Number(1)) },
        new object[] { $"{{ {EscapeString("string")} }}", Completion.NormalCompletion(new String("string")) },
        new object[] { "{ 1 + 1 }", Completion.NormalCompletion(new Number(2)) },
        new object[] { "{ true ; }", Completion.NormalCompletion(new Boolean(true)) },

        // Tests for GreaterThanEquals
        CreateGreaterThanEqualsTestCase(EscapeString("a"), EscapeString("b"), false),
        CreateGreaterThanEqualsTestCase(EscapeString("a"), EscapeString("bc"), false),
        CreateGreaterThanEqualsTestCase(EscapeString("aaa"), EscapeString("aaa"), true),
        CreateGreaterThanEqualsTestCase(EscapeString("aaaa"), EscapeString("aaa"), true),
        CreateGreaterThanEqualsTestCase(EscapeString("aaa"), EscapeString("aaaa"), false),
        CreateGreaterThanEqualsTestCase("0", "0", true),
        CreateGreaterThanEqualsTestCase("0", "1", false),
        CreateGreaterThanEqualsTestCase("1", "0", true),
    };

    static private object[] CreateBooleanLiteralTestCase(bool value)
    {
        return new object[] { value ? "true" : "false", Completion.NormalCompletion(new Boolean(value)) };
    }

    static private object[] CreateNumericLiteralTestCase(double value)
    {
        return new object[] { value.ToString(), Completion.NormalCompletion(new Number(value)) };
    }

    static private object[] CreateStringLiteralTestCase(string value, char quote = '"')
    {
        return new object[] { EscapeString(value, quote), Completion.NormalCompletion(new String(value)) };
    }

    static private object[] CreateStringConcatinationTestCase(string lhs, string rhs, string expected)
    {
        return new object[] { $"{lhs} + {rhs}", Completion.NormalCompletion(new String(expected)) };
    }

    static private object[] CreateAdditionTestCase(double lhs, double rhs, double expected)
    {
        return new object[] { $"{lhs} + {rhs}", Completion.NormalCompletion(new Number(expected)) };
    }

    static private object[] CreateGreaterThanEqualsTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} >= {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    [TestCaseSource(nameof(astTestCases))]
    public void ScriptEvaluation_ReturnsExpectedCompletionAndValue(string testCase, Completion expectedCompletion)
    {
        // Arrange
        var script = ParseScript(testCase);

        // Act
        var actualCompletion = script.ScriptEvaluation();

        // Assert
        Assert.That(actualCompletion, Is.EqualTo(expectedCompletion));
    }

    [Test]
    public void NullLiteral_Evalute_ReturnsNormalCompletion_WithGlobalVMNullValue()
    {
        // Arrange
        var script = ParseScript("null");

        // Act
        var completion = script.ScriptEvaluation();

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        // NOTE: This assert makes sure we use the vm's global null value
        Assert.That(completion.Value, Is.SameAs(script.VM.Null));
    }

    [Test]
    public void BreakStatement_Evaluate_ReturnsBreakCompletion_WithNoTarget_WhenProvidingBreak_WithNoTarget()
    {
        // Arrange
        var vm = new VM();
        var breakStatement = new BreakStatement(null);

        // Act
        var completion = breakStatement.Evaluate(vm);

        // Assert
        Assert.That(completion.IsBreakCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(vm.Empty));
        Assert.That(completion.Target, Is.Empty);
    }

    [Test]
    public void BreakStatement_Evaluate_ReturnsBreakCompletion_WithTarget_WhenProvidingBreak_WithTarget()
    {
        // Arrange
        const string expectedTarget = "target";
        var vm = new VM();
        var identifier = new Identifier(expectedTarget);
        var breakStatement = new BreakStatement(identifier);

        // Act
        var completion = breakStatement.Evaluate(vm);

        // Assert
        Assert.That(completion.IsBreakCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(vm.Empty));
        Assert.That(completion.Target, Is.EqualTo(expectedTarget));
    }

    [Test]
    public void ContinueStatement_Evaluate_ReturnsContinueCompletion_WithNoTarget_WhenProvidingContinue_WithNoTarget()
    {
        // Arrange
        var vm = new VM();
        var continueStatement = new ContinueStatement(null);

        // Act
        var completion = continueStatement.Evaluate(vm);

        // Assert
        Assert.That(completion.IsContinueCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(vm.Empty));
        Assert.That(completion.Target, Is.Empty);
    }

    [Test]
    public void ContinueStatement_Evaluate_ReturnsContinueCompletion_WithTarget_WhenProvidingContinue_WithTarget()
    {
        // Arrange
        const string expectedTarget = "target";
        var vm = new VM();
        var identifier = new Identifier(expectedTarget);
        var continueStatement = new ContinueStatement(identifier);

        // Act
        var completion = continueStatement.Evaluate(vm);

        // Assert
        Assert.That(completion.IsContinueCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(vm.Empty));
        Assert.That(completion.Target, Is.EqualTo(expectedTarget));
    }

    // FIXME: Replace these manual ast tests with the astTestCases array when we can parse more numbers
    static private readonly object[] normalCompletionBitwiseAndTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(1.0) },
    };

    [TestCaseSource(nameof(normalCompletionBitwiseAndTestCases))]
    public void BitwiseAndExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var bitwiseAndExpression = new BitwiseAndExpression(lhs, rhs);

        // Act
        var completion = bitwiseAndExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionBitwiseOrTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(-1.0) },
    };

    [TestCaseSource(nameof(normalCompletionBitwiseOrTestCases))]
    public void BitwiseOrExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var bitwiseOrExpression = new BitwiseOrExpression(lhs, rhs);

        // Act
        var completion = bitwiseOrExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionBitwiseXorTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(-2.0) },
    };

    [TestCaseSource(nameof(normalCompletionBitwiseXorTestCases))]
    public void BitwiseXorExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var bitwiseXorExpression = new BitwiseXorExpression(lhs, rhs);

        // Act
        var completion = bitwiseXorExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionDivisionTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(-1.0) },
    };

    [TestCaseSource(nameof(normalCompletionDivisionTestCases))]
    public void DivisionExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var divisionExpression = new DivisionExpression(lhs, rhs);

        // Act
        var completion = divisionExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionExponentiationTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(1.0) },
    };

    [TestCaseSource(nameof(normalCompletionExponentiationTestCases))]
    public void ExponentiationExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var exponentiationExpression = new ExponentiationExpression(lhs, rhs);

        // Act
        var completion = exponentiationExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionLeftShiftTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(2.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(-2147483648.0) },
    };

    [TestCaseSource(nameof(normalCompletionLeftShiftTestCases))]
    public void LeftShiftExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var leftShiftExpression = new LeftShiftExpression(lhs, rhs);

        // Act
        var completion = leftShiftExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionModuloTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(0.0) },
    };

    [TestCaseSource(nameof(normalCompletionModuloTestCases))]
    public void ModuloExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var moduloExpression = new ModuloExpression(lhs, rhs);

        // Act
        var completion = moduloExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionMultiplyTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(-1.0) },
    };

    [TestCaseSource(nameof(normalCompletionMultiplyTestCases))]
    public void MultiplicationExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var multiplicationExpression = new MultiplicationExpression(lhs, rhs);

        // Act
        var completion = multiplicationExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionRightShiftTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(0.0) },
    };

    [TestCaseSource(nameof(normalCompletionRightShiftTestCases))]
    public void RightShiftExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var rightShiftExpression = new RightShiftExpression(lhs, rhs);

        // Act
        var completion = rightShiftExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionSubtractionTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(2.0) },
    };

    [TestCaseSource(nameof(normalCompletionSubtractionTestCases))]
    public void SubtractionExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var subtractionExpression = new SubtractionExpression(lhs, rhs);

        // Act
        var completion = subtractionExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private readonly object[] normalCompletionUnsignedRightShiftTestCases =
    {
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(0.0) },
    };

    [TestCaseSource(nameof(normalCompletionUnsignedRightShiftTestCases))]
    public void UnsignedRightShiftExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var unsignedRightShiftExpression = new UnsignedRightShiftExpression(lhs, rhs);

        // Act
        var completion = unsignedRightShiftExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

    static private string EscapeString(string toEscape, char quote = '"')
    {
        return $"{quote}{toEscape}{quote}";
    }

    static private Script ParseScript(string script)
    {
        return new Parser(script).Parse();
    }
}
