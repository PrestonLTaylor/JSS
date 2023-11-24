using JSS.Lib.AST.Values;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;
using String = JSS.Lib.AST.Values.String;
using JSS.Lib.AST;

namespace JSS.Lib.UnitTests;

internal sealed class ASTTests
{
    [Test]
    public void BooleanLiteral_Evaluate_ReturnsNormalCompletion_WithFalseValue_WhenProvidingFalse()
    {
        // Arrange
        var vm = new VM();
        var booleanLiteral = new BooleanLiteral(false);

        // Act
        var completion = booleanLiteral.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        var booleanValue = completion.Value as Boolean;
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.False);
    }

    [Test]
    public void BooleanLiteral_Evaluate_ReturnsNormalCompletion_WithTrueValue_WhenProvidingTrue()
    {
        // Arrange
        var vm = new VM();
        var booleanLiteral = new BooleanLiteral(true);

        // Act
        var completion = booleanLiteral.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        var booleanValue = completion.Value as Boolean;
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.True);
    }

    [Test]
    public void NullLiteral_Evalute_ReturnsNormalCompletion_WithNullValue()
    {
        // Arrange
        var vm = new VM();
        var nullLiteral = new NullLiteral();
        
        // Act
        var completion = nullLiteral.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        // NOTE: This assert makes sure we use the vm's global null value
        Assert.That(completion.Value, Is.SameAs(vm.Null));
    }

    // FIXME: More test cases when we parse more numbers
    static private readonly List<double> numericLiteralTestCases = new()
    {
        0.0, 
        1.0, 
        123.0, 
        1234567890.0,
    };

    [TestCaseSource(nameof(numericLiteralTestCases))]
    public void NumericLiteral_Evaluate_ReturnsNormalCompletion_WithExpectedNumber(double testCase)
    {
        // Arrange
        var vm = new VM();
        var numericLiteral = new NumericLiteral(testCase);

        // Act
        var completion = numericLiteral.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        var numberValue = completion.Value as Number;
        Assert.That(numberValue, Is.Not.Null);
        Assert.That(numberValue.Value, Is.EqualTo(testCase));
    }

    // FIXME: More test cases when we parse more strings
    static private readonly List<string> stringLiteralTestCases = new()
    {
        "this is a string literal",
        "\"'\"", 
        "'",
        "'\"'", 
        "\"",
    };

    [TestCaseSource(nameof(stringLiteralTestCases))]
    public void StringLiteral_Evaluate_ReturnsNormalCompletion_WithExpectedString(string testCase)
    {
        // Arrange
        var vm = new VM();
        var stringLiteral = new StringLiteral(testCase);
        
        // Act
        var completion = stringLiteral.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);

        var stringValue = completion.Value as String;
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo(testCase));
    }

    // FIXME: More test case coverage for binary expressions
    static private readonly object[] normalCompletionAdditionTestCases =
    {
        // String Concatination tests
        new object[] { new StringLiteral("lhs"), new StringLiteral("rhs"), new String("lhsrhs") },
        new object[] { new StringLiteral(""), new NumericLiteral(1.0), new String("1") },
        new object[] { new NumericLiteral(1.0), new StringLiteral(""), new String("1") },

        // Numeric tests
        new object[] { new NumericLiteral(1.0), new NumericLiteral(1.0), new Number(2.0) },
        new object[] { new NumericLiteral(1.0), new NumericLiteral(-1.0), new Number(0.0) },
    };

    [TestCaseSource(nameof(normalCompletionAdditionTestCases))]
    public void AdditionExpression_Evaluate_ReturnsNormalCompletion_WithExpectedResult(IExpression lhs, IExpression rhs, Value expectedValue)
    {
        // Arrange
        var vm = new VM();
        var additionExpression = new AdditionExpression(lhs, rhs);

        // Act
        var completion = additionExpression.Evaluate(vm);

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.EqualTo(expectedValue));
    }

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
}
