using System.Diagnostics.CodeAnalysis;

namespace JSS.Lib;

internal sealed class AssertionException : Exception
{
    public AssertionException() : base() { }
    public AssertionException(string? message) : base(message) { }
    public AssertionException(string? message, Exception? innerException) : base(message, innerException) { }

    static public void Assert([DoesNotReturnIf(false)] bool shouldBeTrue, string message)
    {
        if (shouldBeTrue) return;

        throw new AssertionException($"Assertion Failed! {message}");
    }
}
