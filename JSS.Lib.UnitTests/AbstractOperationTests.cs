using FluentAssertions;
using JSS.Lib.AST;
using JSS.Lib.AST.Literal;
using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.UnitTests;

internal sealed class AbstractOperationTests
{
    static private readonly List<Value> toPrimitiveIdentityTestCases = new()
    {
        Null.The,
        false,
        true,
        0.0,
        1.0,
        123456789.0,
        "",
        "'",
        "\"",
        "This is a string literal",
    };

    [TestCaseSource(nameof(toPrimitiveIdentityTestCases))]
    public void ToPrimitive_ReturnsNormalCompletion_WithTheSameValue_WhenNotProvidingAnObject(Value expected)
    {
        // Arrange

        // Act
        var completion = expected.ToPrimitive();

        // Assert
        completion.IsNormalCompletion().Should().BeTrue();
        completion.Value.Should().Be(expected);
    }

    static private readonly object[] valueToExpectedStringTestCases =
    {
        new object[] { Null.The, "null" },
        new object[] { new Boolean(false), "false" },
        new object[] { new Boolean(true), "true" },
        new object[] { new Number(0.0), 0.0.ToString() },
        new object[] { new Number(1.0), 1.0.ToString() },
        new object[] { new Number(123456789.0), 123456789.0.ToString() },
        new object[] { new String(""), "" },
        new object[] { new String("'"), "'" },
        new object[] { new String("\""), "\"" },
        new object[] { new String("This is a string literal"), "This is a string literal" },
    };

    [TestCaseSource(nameof(valueToExpectedStringTestCases))]
    public void ToString_ReturnsNormalCompletion_WithExpectedString_WhenProvidingValue(Value testCase, string expectedString)
    {
        // Arrange

        // Act
        var asString = testCase.ToStringJS();

        // Assert
        asString.IsAbruptCompletion().Should().BeFalse();
        asString.Value.Should().Be(expectedString);
    }

    static private readonly object[] valueToExpectedNumberTestCases = new object[]
    {
        new object[] { Null.The, 0.0 },
        new object[] { new Boolean(false), 0.0 },
        new object[] { new Boolean(true), 1.0 },
        new object[] { new Number(0.0), 0.0 },
        new object[] { new Number(1.0), 1.0 },
        new object[] { new Number(123456789.0), 123456789.0 },
        new object[] { new String("0.0"), 0.0 },
        new object[] { new String("1.0"), 1.0 },
        new object[] { new String("This is a string literal"), double.NaN },
    };

    [TestCaseSource(nameof(valueToExpectedNumberTestCases))]
    public void ToNumber_ReturnsNormalCompletion_WithExpectedNumber_WhenProvidingValue(Value testCase, double expectedNumber)
    {
        // Arrange

        // Act
        var asNumber = testCase.ToNumber();

        // Assert
        asNumber.IsAbruptCompletion().Should().BeFalse();
        asNumber.Value.Should().Be(expectedNumber);
    }

    [TestCaseSource(nameof(valueToExpectedNumberTestCases))]
    public void ToNumeric_ReturnsNormalCompletion_WithExpectedNumber_WhenProvidingValue(Value testCase, double expectedNumber)
    {
        // Arrange

        // Act
        var asNumeric = testCase.ToNumeric();

        // Assert
        asNumeric.IsNormalCompletion().Should().BeTrue();

        var numberValue = asNumeric.Value as Number;
        numberValue.Should().NotBeNull();
        numberValue!.Value.Should().Be(expectedNumber);
    }

    static private readonly object[] unaryMinusValueToExpectedNumberTestCases = new object[]
    {
        new object[] { new Number(1.0), -1.0 },
        new object[] { new Number(-1.0), 1.0 },
        new object[] { new Number(double.NaN), double.NaN },
    };

    [TestCaseSource(nameof(unaryMinusValueToExpectedNumberTestCases))]
    public void UnaryMinus_ReturnsExpectedNumber_WhenProvidingValue(Number testCase, double expectedNumber)
    {
        // Arrange

        // Act
        var result = Number.UnaryMinus(testCase);

        // Assert
        result?.Value.Should().Be(expectedNumber);
    }

    // FIXME: Individual test cases for the underlying abstract operations in ApplyStringOrNumericBinaryOperator 
    // FIXME: More test case coverage
    static private readonly object[] binaryExpressionToExpectedValueTestCases =
    {
        // String Concatination tests
        new object[] { new String("lhs"), BinaryOpType.Add, new String("rhs"), new String("lhsrhs") },
        new object[] { new String(""), BinaryOpType.Add, new Number(1.0), new String("1") },
        new object[] { new Number(1.0), BinaryOpType.Add, new String(""), new String("1") },

        // Numeric tests
        new object[] { new Number(1.0), BinaryOpType.Exponentiate, new Number(1.0), new Number(1.0) },
        new object[] { new Number(1.0), BinaryOpType.Multiply, new Number(1.0), new Number(1.0) },
        new object[] { new Number(1.0), BinaryOpType.Divide, new Number(1.0), new Number(1.0) },
        new object[] { new Number(1.0), BinaryOpType.Remainder, new Number(1.0), new Number(0.0) },
        new object[] { new Number(1.0), BinaryOpType.Add, new Number(1.0), new Number(2.0) },
        new object[] { new Number(1.0), BinaryOpType.Subtract, new Number(1.0), new Number(0.0) },
        new object[] { new Number(1.0), BinaryOpType.LeftShift, new Number(1.0), new Number(2.0) },
        new object[] { new Number(1.0), BinaryOpType.SignedRightShift, new Number(1.0), new Number(0.0) },
        new object[] { new Number(1.0), BinaryOpType.UnsignedRightShift, new Number(1.0), new Number(0.0) },
        new object[] { new Number(1.0), BinaryOpType.BitwiseAND, new Number(1.0), new Number(1.0) },
        new object[] { new Number(1.0), BinaryOpType.BitwiseXOR, new Number(1.0), new Number(0.0) },
        new object[] { new Number(1.0), BinaryOpType.BitwiseOR, new Number(1.0), new Number(1.0) },
    };

    [TestCaseSource(nameof(binaryExpressionToExpectedValueTestCases))]
    public void ApplyStringOrNumericBinaryOperator_ReturnsNormalCompletion_WithExpectedValue(Value lhs, BinaryOpType op, Value rhs, Value expectedValue)
    {
        // Arrange

        // Act
        var result = IExpression.ApplyStringOrNumericBinaryOperator(lhs, op, rhs);

        // Assert
        result.IsNormalCompletion().Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    static private readonly object[] binaryAstExpressionToExpectedValueTestCases =
    {
        // String Concatination tests
        new object[] { new StringLiteral("lhs"), BinaryOpType.Add, new StringLiteral("rhs"), new String("lhsrhs") },
        new object[] { new StringLiteral(""), BinaryOpType.Add, new NumericLiteral(1.0), new String("1") },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Add, new StringLiteral(""), new String("1") },

        // Numeric tests
        new object[] { new NumericLiteral(1.0), BinaryOpType.Exponentiate, new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Multiply, new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Divide, new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Remainder, new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Add, new NumericLiteral(1.0), new Number(2.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.Subtract, new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.LeftShift, new NumericLiteral(1.0), new Number(2.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.SignedRightShift, new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.UnsignedRightShift, new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.BitwiseAND, new NumericLiteral(1.0), new Number(1.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.BitwiseXOR, new NumericLiteral(1.0), new Number(0.0) },
        new object[] { new NumericLiteral(1.0), BinaryOpType.BitwiseOR, new NumericLiteral(1.0), new Number(1.0) },
    };

    [TestCaseSource(nameof(binaryAstExpressionToExpectedValueTestCases))]
    public void EvaluateStringOrNumericBinaryExpression_ReturnsNormalCompletion_WithExpectedValue(IExpression lhs, BinaryOpType op, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var realm = new Realm();
        var vm = new VM(realm);

        // Act
        var result = IExpression.EvaluateStringOrNumericBinaryExpression(vm, lhs, op, rhs);

        // Assert
        result.IsNormalCompletion().Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    // FIXME: Tests for Number::lessThan
    static private readonly object[] isLessThanLhsAndRhsToExpectedResultTestCases =
    {
        new object[] { new String("a"), new String("b"), true },
        new object[] { new String("a"), new String("bc"), true },
        new object[] { new String("b"), new String("a"), false },
        new object[] { new String("aaa"), new String("aaa"), false },
        new object[] { new String("aaaa"), new String("aaa"), false },
        new object[] { new String("aaa"), new String("aaaa"), true },

        new object[] { new Number(0), new Number(0), false },
        new object[] { new Number(0), new Number(1), true },
        new object[] { new Number(1), new Number(0), false },
    };

    [TestCaseSource(nameof(isLessThanLhsAndRhsToExpectedResultTestCases))]
    public void IsLessThan_ReturnsNormalCompletion_WithExpectedResult(Value lhs, Value rhs, bool expectedResult)
    {
        // Arrange
        var expectedValue = new Boolean(expectedResult);

        // Act
        // NOTE: leftFirst has no visiable side effect in these tests
        var completion = Value.IsLessThan(lhs, rhs, false);

        // Assert
        completion.IsNormalCompletion().Should().BeTrue();

        completion.Value.Should().Be(expectedValue);
    }

    static private readonly object[] toBooleanValueToExpectedResultTestCases =
    {
        new object[] { new Boolean(false), false },
        new object[] { new Boolean(true), true },

        new object[] { Undefined.The, false },
        new object[] { Null.The, false },
        new object[] { new Number(0), false },
        new object[] { new Number(double.NaN), false },
        new object[] { new String(""), false },

        new object[] { new String("a"), true },
        new object[] { new Number(1), true },
    };

    [TestCaseSource(nameof(toBooleanValueToExpectedResultTestCases))]
    public void ToBoolean_ReturnsBoolean_WithExpectedResult(Value testCase, bool expectedResult)
    {
        // Arrange
        var expectedValue = new Boolean(expectedResult);

        // Act
        var result = testCase.ToBoolean();

        // Assert
        result.Should().Be(expectedValue);
    }

    // FIXME: Tests for Number::equal and SameValueNonNumber
    static private readonly object[] isStrictlyEqualLhsAndRhsToExpectedResultTestCases =
    {
        new object[] { Null.The, Undefined.The, false },
        new object[] { Undefined.The, Null.The, false },
        new object[] { Undefined.The, new Number(1), false },
        new object[] { Undefined.The, new String("1"), false },
        new object[] { new Number(1), new String("1"), false },
        new object[] { new String("1"), new Number(1), false },
        new object[] { new String("1"), new String("2"), false },
        new object[] { new Number(1), new Number(2), false },
        new object[] { new Boolean(false), new Boolean(true), false },
        new object[] { Undefined.The, Undefined.The, true },
        new object[] { Null.The, Null.The, true },
        new object[] { new Boolean(false), new Boolean(false), true },
        new object[] { new Boolean(true), new Boolean(true), true },
        new object[] { new String("1"), new String("1"), true },
        new object[] { new Number(1), new Number(1), true },
    };

    [TestCaseSource(nameof(isStrictlyEqualLhsAndRhsToExpectedResultTestCases))]
    public void IsStrictlyEqual_ReturnsBoolean_WithExpectedResult(Value lhs, Value rhs, bool expectedResult)
    {
        // Arrange
        var expectedValue = new Boolean(expectedResult);

        // Act
        var result = Value.IsStrictlyEqual(lhs, rhs);

        // Assert
        result.Should().Be(expectedValue);
    }

    static private readonly object[] isLooselyEqualLhsAndRhsToExpectedResultTestCases =
    {
        new object[] { Null.The, Undefined.The, true },
        new object[] { Undefined.The, Null.The, true },
        new object[] { Undefined.The, new Number(1), false },
        new object[] { Undefined.The, new String("1"), false },
        new object[] { new Number(1), new String("1"), true },
        new object[] { new String("1"), new Number(1), true },
        new object[] { new String("1"), new String("2"), false },
        new object[] { new Number(1), new Number(2), false },
        new object[] { new Boolean(false), new Boolean(true), false },
        new object[] { Undefined.The, Undefined.The, true },
        new object[] { Null.The, Null.The, true },
        new object[] { new Boolean(false), new Boolean(false), true },
        new object[] { new Boolean(true), new Boolean(true), true },
        new object[] { new String("1"), new String("1"), true },
        new object[] { new Number(1), new Number(1), true },
    };

    [TestCaseSource(nameof(isLooselyEqualLhsAndRhsToExpectedResultTestCases))]
    public void IsLooselyEqual_ReturnsNormalCompletion_WithExpectedResult(Value lhs, Value rhs, bool expectedResult)
    {
        // Arrange
        var expectedValue = new Boolean(expectedResult);

        // Act
        var completion = Value.IsLooselyEqual(lhs, rhs);

        // Assert
        completion.IsNormalCompletion().Should().BeTrue();

        completion.Value.Should().Be(expectedValue);
    }

    static private readonly object[] loopContinuesValueToExpectedResultTestCases =
    {
        new object[] { Completion.NormalCompletion(Undefined.The), true },
        new object[] { Completion.ThrowCompletion(Undefined.The), false },
        new object[] { Completion.BreakCompletion(Undefined.The, ""), false },
        new object[] { Completion.ReturnCompletion(Undefined.The), false },
        new object[] { Completion.ContinueCompletion(Undefined.The, ""), true },
    };

    [TestCaseSource(nameof(loopContinuesValueToExpectedResultTestCases))]
    public void LoopContinues_ReturnsExpectedBoolean_WhenProvidedCompletion(Completion completion, bool expectedResult)
    {
        // Arrange
        var expectedValue = new Boolean(expectedResult);

        // Act
        var result = completion.LoopContinues();

        // Assert
        result.Should().Be(expectedValue);
    }
}
