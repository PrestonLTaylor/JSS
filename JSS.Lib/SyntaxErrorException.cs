namespace JSS.Lib;

// NOTE: This is a exception used in the parser to "simulate" a SyntaxError being thrown in JS
public sealed class SyntaxErrorException : Exception
{
    public SyntaxErrorException()
    {
    }

    public SyntaxErrorException(string message)
        : base(message)
    {
    }

    public SyntaxErrorException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
