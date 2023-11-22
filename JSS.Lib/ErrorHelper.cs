namespace JSS.Lib;

internal enum ErrorType
{
    ConstWithoutInitializer,
    TryWithoutCatchOrFinally,
    UnaryLHSOfExponentiation,
    UnexpectedEOF,
    UnexpectedToken,
    UnknownSyntaxError,
}

internal sealed class ErrorHelper
{
    // FIXME: Add Line/Char numbers to errors
    static public void ThrowSyntaxError(ErrorType type, params object?[] args)
    {
        try
        {
            throw CreateSyntaxError(type, args);
        } 
        catch (KeyNotFoundException)
        {
            ThrowSyntaxError(ErrorType.UnknownSyntaxError, type);
        }
    }

    static public SyntaxErrorException CreateSyntaxError(ErrorType type, params object?[] args)
    {
        var formatString = errorTypeToFormatString[type];
        var formattedString = string.Format(formatString, args);
        return new SyntaxErrorException(formattedString);
    }

    static private readonly Dictionary<ErrorType, string> errorTypeToFormatString = new()
    {
        { ErrorType.ConstWithoutInitializer, "SyntaxError: Missing initializer in const declaration" },
        { ErrorType.TryWithoutCatchOrFinally, "SyntaxError: Try statement without catch or finally blocks" },
        { ErrorType.UnaryLHSOfExponentiation, "SyntaxError: Unary operator as lhs of exponentiation expression. Consider using parenthesis to disambiguate precidence" },
        { ErrorType.UnexpectedToken, "SyntaxError: Unexpected token '{0}'" },
        { ErrorType.UnexpectedEOF, "SyntaxError: Unexpected end of file" },
        { ErrorType.UnknownSyntaxError, "SyntaxError: Unknown SyntaxError of type {0}" }
    };
}
