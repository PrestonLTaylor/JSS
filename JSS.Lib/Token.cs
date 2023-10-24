namespace JSS.Lib;

internal enum TokenType
{
    LineTerminator,
}

// FIXME: Support character/line numbers with tokens
internal struct Token
{
    public TokenType type;
    public string data;
}
