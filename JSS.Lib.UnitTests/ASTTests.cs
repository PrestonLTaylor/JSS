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

        // Tests for GreaterThan
        CreateGreaterThanTestCase(EscapeString("a"), EscapeString("b"), false),
        CreateGreaterThanTestCase(EscapeString("a"), EscapeString("bc"), false),
        CreateGreaterThanTestCase(EscapeString("aaa"), EscapeString("aaa"), false),
        CreateGreaterThanTestCase(EscapeString("aaaa"), EscapeString("aaa"), true),
        CreateGreaterThanTestCase(EscapeString("aaa"), EscapeString("aaaa"), false),
        CreateGreaterThanTestCase("0", "0", false),
        CreateGreaterThanTestCase("0", "1", false),
        CreateGreaterThanTestCase("1", "0", true),

        // Tests for LessThanEquals
        CreateLessThanEqualsTestCase(EscapeString("a"), EscapeString("b"), true),
        CreateLessThanEqualsTestCase(EscapeString("a"), EscapeString("bc"), true),
        CreateLessThanEqualsTestCase(EscapeString("aaa"), EscapeString("aaa"), true),
        CreateLessThanEqualsTestCase(EscapeString("aaaa"), EscapeString("aaa"), false),
        CreateLessThanEqualsTestCase(EscapeString("aaa"), EscapeString("aaaa"), true),
        CreateLessThanEqualsTestCase("0", "0", true),
        CreateLessThanEqualsTestCase("0", "1", true),
        CreateLessThanEqualsTestCase("1", "0", false),

        // Tests for LessThan
        CreateLessThanTestCase(EscapeString("a"), EscapeString("b"), true),
        CreateLessThanTestCase(EscapeString("a"), EscapeString("bc"), true),
        CreateLessThanTestCase(EscapeString("aaa"), EscapeString("aaa"), false),
        CreateLessThanTestCase(EscapeString("aaaa"), EscapeString("aaa"), false),
        CreateLessThanTestCase(EscapeString("aaa"), EscapeString("aaaa"), true),
        CreateLessThanTestCase("0", "0", false),
        CreateLessThanTestCase("0", "1", true),
        CreateLessThanTestCase("1", "0", false),

        // Tests for LogicalNOT
        CreateLogicalNOTTestCase("false", true),
        CreateLogicalNOTTestCase("true", false),
        CreateLogicalNOTTestCase("null", true),
        CreateLogicalNOTTestCase("0", true),
        CreateLogicalNOTTestCase(EscapeString(""), true),
        CreateLogicalNOTTestCase(EscapeString("a"), false),
        CreateLogicalNOTTestCase("1", false),

        // Tests for LogicalAND
        CreateLogicalANDTestCase("false", "false", new Boolean(false)),
        CreateLogicalANDTestCase("true", "false", new Boolean(false)),
        CreateLogicalANDTestCase("false", "true", new Boolean(false)),
        CreateLogicalANDTestCase("true", "true", new Boolean(true)),
        CreateLogicalANDTestCase("0", "true", new Number(0)),
        CreateLogicalANDTestCase("true", "0", new Number(0)),
        CreateLogicalANDTestCase(EscapeString(""), "true", new String("")),
        CreateLogicalANDTestCase("true", EscapeString(""), new String("")),
        CreateLogicalANDTestCase(EscapeString("a"), EscapeString("a"), new String("a")),
        CreateLogicalANDTestCase("1", "1", new Number(1)),

        // Tests for LogicalOR
        CreateLogicalORTestCase("false", "false", new Boolean(false)),
        CreateLogicalORTestCase("true", "false", new Boolean(true)),
        CreateLogicalORTestCase("false", "true", new Boolean(true)),
        CreateLogicalORTestCase("true", "true", new Boolean(true)),
        CreateLogicalORTestCase("0", "true", new Boolean(true)),
        CreateLogicalORTestCase("true", "0", new Boolean(true)),
        CreateLogicalORTestCase(EscapeString(""), "true", new Boolean(true)),
        CreateLogicalORTestCase("true", EscapeString(""), new Boolean(true)),
        CreateLogicalORTestCase(EscapeString("a"), EscapeString("a"), new String("a")),
        CreateLogicalORTestCase("1", "1", new Number(1)),

        // Tests for LooseEquality
        CreateLooselyEqualTestCase("1", EscapeString("1"), true),
        CreateLooselyEqualTestCase(EscapeString("1"), "1", true),
        CreateLooselyEqualTestCase(EscapeString("1"), EscapeString("2"), false),
        CreateLooselyEqualTestCase("false", "true", false),
        CreateLooselyEqualTestCase("null", "null", true),
        CreateLooselyEqualTestCase("false", "false", true),
        CreateLooselyEqualTestCase("true", "true", true),
        CreateLooselyEqualTestCase(EscapeString("1"), EscapeString("1"), true),
        CreateLooselyEqualTestCase("1", "1", true),
        
        // Tests for LooseInequality
        CreateLooselyInequalTestCase("1", EscapeString("1"), false),
        CreateLooselyInequalTestCase(EscapeString("1"), "1", false),
        CreateLooselyInequalTestCase(EscapeString("1"), EscapeString("2"), true),
        CreateLooselyInequalTestCase("false", "true", true),
        CreateLooselyInequalTestCase("null", "null", false),
        CreateLooselyInequalTestCase("false", "false", false),
        CreateLooselyInequalTestCase("true", "true", false),
        CreateLooselyInequalTestCase(EscapeString("1"), EscapeString("1"), false),
        CreateLooselyInequalTestCase("1", "1", false),

        // Tests for StrictEquality
        CreateStrictEqualTestCase("1", EscapeString("1"), false),
        CreateStrictEqualTestCase(EscapeString("1"), "1", false),
        CreateStrictEqualTestCase(EscapeString("1"), EscapeString("2"), false),
        CreateStrictEqualTestCase("false", "true", false),
        CreateStrictEqualTestCase("null", "null", true),
        CreateStrictEqualTestCase("false", "false", true),
        CreateStrictEqualTestCase("true", "true", true),
        CreateStrictEqualTestCase(EscapeString("1"), EscapeString("1"), true),
        CreateStrictEqualTestCase("1", "1", true),

        // Tests for StrictInequality
        CreateStrictInequalTestCase("1", EscapeString("1"), true),
        CreateStrictInequalTestCase(EscapeString("1"), "1", true),
        CreateStrictInequalTestCase(EscapeString("1"), EscapeString("2"), true),
        CreateStrictInequalTestCase("false", "true", true),
        CreateStrictInequalTestCase("null", "null", false),
        CreateStrictInequalTestCase("false", "false", false),
        CreateStrictInequalTestCase("true", "true", false),
        CreateStrictInequalTestCase(EscapeString("1"), EscapeString("1"), false),
        CreateStrictInequalTestCase("1", "1", false),
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

    static private object[] CreateGreaterThanTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} > {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateLessThanEqualsTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} <= {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateLessThanTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} < {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateLogicalNOTTestCase(string value, bool expected)
    {
        return new object[] { $"!{value}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateLogicalANDTestCase(string lhs, string rhs, Value expected)
    {
        return new object[] { $"{lhs} && {rhs}", Completion.NormalCompletion(expected) };
    }

    static private object[] CreateLogicalORTestCase(string lhs, string rhs, Value expected)
    {
        return new object[] { $"{lhs} || {rhs}", Completion.NormalCompletion(expected) };
    }

    static private object[] CreateLooselyEqualTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} == {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateLooselyInequalTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} != {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateStrictEqualTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} === {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
    }

    static private object[] CreateStrictInequalTestCase(string lhs, string rhs, bool expected)
    {
        return new object[] { $"{lhs} !== {rhs}", Completion.NormalCompletion(new Boolean(expected)) };
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
