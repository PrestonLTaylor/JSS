using JSS.Lib.AST.Value;
using Boolean = JSS.Lib.AST.Value.Boolean;
using String = JSS.Lib.AST.Value.String;

namespace JSS.Lib.UnitTests;

internal sealed class AbstractOperationTests
{
    static private readonly List<Value> toPrimitiveIdentityTestCases = new()
    {
        new Null(),
        new Boolean { Value = false },
        new Boolean { Value = true },
        new Number { Value = 0.0 },
        new Number { Value = 1.0 },
        new Number { Value = 123456789.0 },
        new Number { Value = 123456789.0 },
        new String(""),
        new String("'"),
        new String("\""),
        new String("This is a string literal"),
    };

    [TestCaseSource(nameof(toPrimitiveIdentityTestCases))]
    public void ToPrimitive_ReturnsTheSameValue_WhenNotProvidingAnObject(Value expected)
    {
        // Arrange

        // Act
        var completion = expected.ToPrimitive();

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.SameAs(expected));
    }
}
