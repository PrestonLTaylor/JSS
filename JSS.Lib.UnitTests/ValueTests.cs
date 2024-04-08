using FluentAssertions;
using JSS.Lib.AST.Values;

namespace JSS.Lib.UnitTests;

internal sealed class ValueTests
{
    // Tests for 7.1.6 ToInt32 ( argument ), https://tc39.es/ecma262/#sec-toint32
    static private readonly object[] toInt32TestCases =
    {
        new object[] { 0.0, 0.0 },
        new object[] { 1.0, 1.0 },
        new object[] { -1.0, -1.0 },
        new object[] { uint.MaxValue, -1 },
        new object[] { int.MaxValue, int.MaxValue },
        new object[] { (uint)int.MaxValue + 1, int.MinValue },
        new object[] { double.MaxValue, 0 },
        new object[] { double.MinValue, 0 },
    };

    [TestCaseSource(nameof(toInt32TestCases))]
    public void ToInt32_ReturnsNormalCompletion_WithExpectedResult(double input, double expected)
    {
        // Arrange
        Value inputValue = input;

        // Act
        var completion = inputValue.ToInt32();

        // Assert
        completion.IsNormalCompletion().Should().BeTrue();

        var actual = completion.Value.AsNumber();
        actual.Value.Should().Be(expected);
    }

    // 7.1.7 ToUint32 ( argument ), https://tc39.es/ecma262/#sec-touint32
    static private readonly object[] toUint32TestCases =
    {
        new object[] { 0.0, 0.0 },
        new object[] { 1.0, 1.0 },
        new object[] { -1.0, uint.MaxValue },
        new object[] { uint.MaxValue, uint.MaxValue },
        new object[] { int.MaxValue, int.MaxValue },
        new object[] { (uint)int.MaxValue + 1, (uint)int.MaxValue + 1 },
        new object[] { double.MaxValue, 0 },
        new object[] { double.MinValue, 0 },
    };

    [TestCaseSource(nameof(toUint32TestCases))]
    public void ToUint32_ReturnsNormalCompletion_WithExpectedResult(double input, double expected)
    {
        // Arrange
        Value inputValue = input;

        // Act
        var completion = inputValue.ToUint32();

        // Assert
        completion.IsNormalCompletion().Should().BeTrue();

        var actual = completion.Value.AsNumber();
        actual.Value.Should().Be(expected);
    }
}
