using FluentAssertions;
using JSS.Lib.AST.Values;

namespace JSS.Lib.UnitTests;

internal sealed class ValueTests
{
    // Tests for 7.1.6 ToInt32 ( argument ), https://tc39.es/ecma262/#sec-toint32
    static private readonly object[] toInt32TestCases =
    {
        new object[] { 0.0, 0 },
        new object[] { 1.0, 1 },
        new object[] { -1.0, -1 },
        new object[] { uint.MaxValue, -1 },
        new object[] { int.MaxValue, int.MaxValue },
        new object[] { (uint)int.MaxValue + 1, int.MinValue },
        new object[] { double.MaxValue, 0 },
        new object[] { double.MinValue, 0 },
    };

    [TestCaseSource(nameof(toInt32TestCases))]
    public void ToInt32_ReturnsNormalCompletion_WithExpectedResult(double input, int expected)
    {
        // Arrange
        Value inputValue = input;

        // Act
        var abruptOr = inputValue.ToInt32();

        // Assert
        abruptOr.IsAbruptCompletion().Should().BeFalse();
        abruptOr.Value.Should().Be(expected);
    }

    // 7.1.7 ToUint32 ( argument ), https://tc39.es/ecma262/#sec-touint32
    static private readonly object[] toUint32TestCases =
    {
        new object[] { 0.0, 0U },
        new object[] { 1.0, 1U },
        new object[] { -1.0, uint.MaxValue },
        new object[] { uint.MaxValue, uint.MaxValue },
        new object[] { int.MaxValue, (uint)int.MaxValue },
        new object[] { (uint)int.MaxValue + 1, (uint)int.MaxValue + 1 },
        new object[] { double.MaxValue, 0U },
        new object[] { double.MinValue, 0U },
    };

    [TestCaseSource(nameof(toUint32TestCases))]
    public void ToUint32_ReturnsNormalCompletion_WithExpectedResult(double input, uint expected)
    {
        // Arrange
        Value inputValue = input;

        // Act
        var abruptOr = inputValue.ToUint32();

        // Assert
        abruptOr.IsAbruptCompletion().Should().BeFalse();
        abruptOr.Value.Should().Be(expected);
    }
}
