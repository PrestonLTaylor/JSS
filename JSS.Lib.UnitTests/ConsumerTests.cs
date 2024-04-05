using FluentAssertions;

namespace JSS.Lib.UnitTests;

internal sealed class ConsumerTests
{
    [Test]
    public void CanConsume_ReturnsFalse_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act

        // Assert
        consumer.CanConsume().Should().BeFalse();
    }

    [Test]
    public void CanConsume_ReturnsTrue_WhenProvidedNonEmptyString()
    {
        // Arrange
        const string testString = "a";
        var consumer = new Consumer(testString);

        // Act

        // Assert
        consumer.CanConsume().Should().BeTrue();
    }

    [Test]
    public void CanConsume_ReturnsFalse_WhenStringIsFullyConsumed()
    {
        // Arrange
        const string testString = "a";
        var consumer = new Consumer(testString);

        // Act
        consumer.Consume();

        // Assert
        consumer.CanConsume().Should().BeFalse();
    }

    [Test]
    public void CanConsume_ReturnsTrue_WhenStringIsPartiallyConsumed()
    {
        // Arrange
        const string testString = "ab";
        var consumer = new Consumer(testString);

        // Act
        consumer.Consume();

        // Assert
        consumer.CanConsume().Should().BeTrue();
    }

    [Test]
    public void CanConsume_ReturnsFalse_WhenOffsetIsOutsideOfString()
    {
        // Arrange
        const string testString = "ab";
        var consumer = new Consumer(testString);

        // Act

        // Assert
        consumer.CanConsume(testString.Length + 1).Should().BeFalse();
    }

    [Test]
    public void Consume_ReturnsFirstCharacterOfString_WhenCalledOnce()
    {
        // Arrange
        const string testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var consumed = consumer.Consume();

        // Assert
        consumed.Should().Be(testString[0]);
    }

    [Test]
    public void Consume_ThrowsIndexOutOfRangeException_WhenCanConsumeIsFalse()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act

        // Assert
        Assert.Throws<IndexOutOfRangeException>(() => consumer.Consume());
    }

    [Test]
    public void ConsumeUntilCanConsumeIsFalse_ProducesTheSameStringInputted()
    {
        // Arrange
        const string expectedString = "abcd";
        var consumer = new Consumer(expectedString);

        // Act
        string actualString = "";
        while (consumer.CanConsume())
        {
            actualString += consumer.Consume();
        }

        // Assert
        actualString.Should().Be(expectedString);
    }

    [Test]
    public void ConsumeWhile_ConsumesUntilPredicateIsFalse()
    {
        // Arrange
        const string testString = "abcde";
        var consumer = new Consumer(testString);

        // Act
        var consumed = consumer.ConsumeWhile((codePoint) =>
        {
            return codePoint != 'e';
        });

        // Assert
        Assert.Multiple(() =>
        {
            consumer.CanConsume().Should().BeTrue();
            consumed.Should().Be("abcd");
            consumer.Peek().Should().Be('e');
        });
    }

    [Test]
    public void ConsumeWhile_DoesNotThrow_WhenFullyConsumingString()
    {
        // Arrange
        const string testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var consumed = consumer.ConsumeWhile((_) => true);

        // Assert
        Assert.Multiple(() =>
        {
            consumer.CanConsume().Should().BeFalse();
            consumed.Should().Be(testString);
        });
    }

    [Test]
    public void TryConsumeString_ReturnsTrue_WhenProvidedEmptyStringOnEmptyConsumer()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act
        var didConsume = consumer.TryConsumeString("");

        // Assert
        didConsume.Should().BeTrue();
    }

    [Test]
    public void TryConsumeString_ReturnsTrue_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("abcd");

        // Act
        var didConsume = consumer.TryConsumeString("");

        // Assert
        didConsume.Should().BeTrue();
    }

    [Test]
    public void TryConsumeString_ReturnsTrue_WhenProvidedSameStrings()
    {
        // Arrange
        var testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var didConsume = consumer.TryConsumeString(testString);

        // Assert
        Assert.Multiple(() =>
        {
            didConsume.Should().BeTrue();
            consumer.CanConsume().Should().BeFalse();
        });
    }

    [Test]
    public void TryConsumeString_ReturnsTrue_WhenProvidedSubString()
    {
        // Arrange
        var subString = "ab";
        var testString = subString + "cd";
        var consumer = new Consumer(testString);

        // Act
        var didConsume = consumer.TryConsumeString(subString);

        // Assert
        Assert.Multiple(() =>
        {
            didConsume.Should().BeTrue();
            consumer.CanConsume().Should().BeTrue();
        });
    }

    [Test]
    public void TryConsumeString_ReturnsFalse_WhenProvidedDifferentSubString()
    {
        // Arrange
        var testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var didConsume = consumer.TryConsumeString("dcba");

        // Assert
        didConsume.Should().BeFalse();
    }

    [Test]
    public void TryConsumeString_ReturnsFalse_ProvidingAString_WhenCanConsumeIsFalse()
    {
        // Arrange
        var testString = "a";
        var consumer = new Consumer(testString);

        // Act
        consumer.Consume();
        var didConsume = consumer.TryConsumeString(testString);

        // Assert
        didConsume.Should().BeFalse();
    }

    [Test]
    public void Peek_ReturnsFirstCharacterOfString_WhenCalled()
    {
        // Arrange
        const string testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var peeked = consumer.Peek();

        // Assert
        peeked.Should().Be(testString[0]);
    }

	[Test]
	public void Peek_ReturnsSecondCharacterOfString_WhenCalledWithOffsetOf1()
	{
		// Arrange
		const string testString = "abcd";
		var consumer = new Consumer(testString);

		// Act
		var peeked = consumer.Peek(1);

		// Assert
		peeked.Should().Be(testString[1]);
	}

	[Test]
    public void Peek_ReturnsTheSameCharacter_WhenCalledMultipleTimes()
    {
        // Arrange
        const string testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var firstPeek = consumer.Peek();
        var secondPeek = consumer.Peek();

        // Assert
        firstPeek.Should().Be(secondPeek);
    }

    [Test]
    public void Peek_DoesntChangeCanConsume_WhenPeekingLastCharacter()
    {
        // Arrange
        const string testString = "a";
        var consumer = new Consumer(testString);

        // Act
        consumer.Peek();

        // Assert
        consumer.CanConsume().Should().BeTrue();
    }

    [Test]
    public void Peek_ThrowsIndexOutOfRangeException_WhenCanConsumeIsFalse()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act

        // Assert
        Assert.Throws<IndexOutOfRangeException>(() => consumer.Peek());
    }

    [Test]
    public void Matches_ReturnsTrue_WhenProvidedEmptyStringOnEmptyConsumer()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act
        var matches = consumer.Matches("");

        // Assert
        matches.Should().BeTrue();
    }

    [Test]
    public void Matches_ReturnsTrue_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("abcd");

        // Act
        var matches = consumer.Matches("");

        // Assert
        matches.Should().BeTrue();
    }

    [Test]
    public void Matches_ReturnsTrue_WhenProvidedSameStrings()
    {
        // Arrange
        var testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var matches = consumer.Matches(testString);

        // Assert
        matches.Should().BeTrue();
    }

    [Test]
    public void Matches_ReturnsTrue_WhenProvidedSubString()
    {
        // Arrange
        var subString = "ab";
        var testString = subString + "cd";
        var consumer = new Consumer(testString);

        // Act
        var matches = consumer.Matches(subString);

        // Assert
        matches.Should().BeTrue();
    }

    [Test]
    public void Matches_ReturnsFalse_WhenProvidedDifferentSubString()
    {
        // Arrange
        var testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var matches = consumer.Matches("dcba");

        // Assert
        matches.Should().BeFalse();
    }

    [Test]
    public void Matches_ReturnsFalse_WhenProvidedABiggerString()
    {
        // Arrange
        var testString = "abcd";
        var consumer = new Consumer(testString);

        // Act
        var matches = consumer.Matches("abcde");

        // Assert
        matches.Should().BeFalse();
    }

    [Test]
    public void Matches_ReturnsFalse_ProvidingAString_WhenCanConsumeIsFalse()
    {
        // Arrange
        var testString = "a";
        var consumer = new Consumer(testString);

        // Act
        consumer.Consume();
        var matches = consumer.Matches(testString);

        // Assert
        matches.Should().BeFalse();
    }
}
