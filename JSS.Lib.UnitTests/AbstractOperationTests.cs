﻿using JSS.Lib.AST.Values;
using JSS.Lib.Execution;
using Boolean = JSS.Lib.AST.Values.Boolean;
using String = JSS.Lib.AST.Values.String;

namespace JSS.Lib.UnitTests;

internal sealed class AbstractOperationTests
{
    static private readonly List<Value> toPrimitiveIdentityTestCases = new()
    {
        new Null(),
        new Boolean(false),
        new Boolean(true),
        new Number(0.0),
        new Number(1.0),
        new Number(123456789.0),
        new String(""),
        new String("'"),
        new String("\""),
        new String("This is a string literal"),
    };

    [TestCaseSource(nameof(toPrimitiveIdentityTestCases))]
    public void ToPrimitive_ReturnsNormalCompletion_WithTheSameValue_WhenNotProvidingAnObject(Value expected)
    {
        // Arrange

        // Act
        var completion = expected.ToPrimitive();

        // Assert
        Assert.That(completion.IsNormalCompletion(), Is.True);
        Assert.That(completion.Value, Is.SameAs(expected));
    }

    // FIXME: Replace other Dictionary test cases with object[]
    static private readonly object[] valueToExpectedStringTestCases =
    {
        new object[] { new Null(), "null" },
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
        Assert.That(asString.IsNormalCompletion(), Is.True);

        var stringValue = asString.Value as String;
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo(expectedString));
    }

    static private readonly object[] valueToExpectedNumberTestCases = new object[]
    {
        new object[] { new Null(), 0.0 },
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
        var vm = new VM();

        // Act
        var asNumber = testCase.ToNumber(vm);

        // Assert
        Assert.That(asNumber.IsNormalCompletion(), Is.True);

        var numberValue = asNumber.Value as Number;
        Assert.That(numberValue, Is.Not.Null);
        Assert.That(numberValue.Value, Is.EqualTo(expectedNumber));
    }
}
