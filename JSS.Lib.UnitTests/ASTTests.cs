using JSS.Lib.AST.Value;
using JSS.Lib.AST.Literal;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Value.Boolean;
using String = JSS.Lib.AST.Value.String;

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
}
