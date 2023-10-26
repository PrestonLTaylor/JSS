namespace JSS.Lib;

internal enum TokenType
{
    LineTerminator,

    // https://tc39.es/ecma262/#prod-ReservedWord
    Await,
    Break,
    Case,
    Catch,
    Class,
    Const,
    Continue,
    Debugger,
    Default,
    Delete,
    Do,
    Else,
    Enum,
    Export,
    Extends,
    False,
    Finally,
    For,
    Function,
    If,
    Import,
    In,
    InstanceOf,
    New,
    Null,
    Return,
    Super,
    Switch,
    This,
    Throw,
    True,
    Try,
    TypeOf,
    Var,
    Void,
    While,
    With,
    Yield,
}

// FIXME: Support character/line numbers with tokens
internal struct Token
{
    public TokenType type;
    public string data;
}
