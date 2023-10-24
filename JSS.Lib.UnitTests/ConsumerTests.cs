﻿namespace JSS.Lib.UnitTests;

internal sealed class ConsumerTests
{
    [Test]
    public void CanConsume_ReturnsFalse_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act

        // Assert
        Assert.That(consumer.CanConsume(), Is.False);
    }

    [Test]
    public void CanConsume_ReturnsTrue_WhenProvidedNonEmptyString()
    {
        // Arrange
        const string testString = "a";
        var consumer = new Consumer(testString);

        // Act

        // Assert
        Assert.That(consumer.CanConsume(), Is.True);
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
        Assert.That(consumer.CanConsume(), Is.False);
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
        Assert.That(consumer.CanConsume(), Is.True);
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
        Assert.That(consumed, Is.EqualTo(testString[0]));
    }

    [Test]
    public void Consume_ThrowsIndexOutOfRangeException_WhenCanConsumeIsFalse()
    {
        // Arrange
        var consumer = new Consumer("");

        // Act
        
        // Assert
        Assert.That(consumer.Consume, Throws.Exception.TypeOf<IndexOutOfRangeException>());
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
        Assert.That(actualString, Is.EqualTo(expectedString));
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
            Assert.That(consumer.CanConsume(), Is.True);
            Assert.That(consumed, Is.EqualTo("abcd"));
            Assert.That(consumer.Peek(), Is.EqualTo('e'));
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
            Assert.That(consumer.CanConsume(), Is.False);
            Assert.That(consumed, Is.EqualTo(testString));
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
        Assert.That(didConsume, Is.True);
    }

    [Test]
    public void TryConsumeString_ReturnsTrue_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("abcd");

        // Act
        var didConsume = consumer.TryConsumeString("");

        // Assert
        Assert.That(didConsume, Is.True);
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
            Assert.That(didConsume, Is.True);
            Assert.That(consumer.CanConsume(), Is.False);
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
            Assert.That(didConsume, Is.True);
            Assert.That(consumer.CanConsume(), Is.True);
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
        Assert.That(didConsume, Is.False);
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
        Assert.That(didConsume, Is.False);
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
        Assert.That(peeked, Is.EqualTo(testString[0]));
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
		Assert.That(peeked, Is.EqualTo(testString[1]));
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
        Assert.That(firstPeek, Is.EqualTo(secondPeek));
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
        Assert.That(consumer.CanConsume(), Is.True);
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
        Assert.That(matches, Is.True);
    }

    [Test]
    public void Matches_ReturnsTrue_WhenProvidedEmptyString()
    {
        // Arrange
        var consumer = new Consumer("abcd");

        // Act
        var matches = consumer.Matches("");

        // Assert
        Assert.That(matches, Is.True);
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
        Assert.That(matches, Is.True);
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
        Assert.That(matches, Is.True);
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
        Assert.That(matches, Is.False);
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
        Assert.That(matches, Is.False);
    }
}
