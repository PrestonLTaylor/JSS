﻿namespace JSS.Lib;

internal enum TokenType
{
    LineTerminator,

    // https://tc39.es/ecma262/#sec-identifier-names
    PrivateIdentifier,
    Identifier,
    
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
    Let,

    // https://tc39.es/ecma262/#prod-Punctuator
    OptionalChaining,
    OpenBrace,
    OpenParen,
    ClosedParen,
    OpenSquare,
    ClosedSquare,
    Dot,
    Spread,
    SemiColon,
    Comma,
    LessThan,
    GreaterThan,
    LessThanEqual,
    GreaterThanEqual,
    EqualEquals,
    NotEquals,
    StrictEqualsEquals,
    StrictNotEquals,
    Plus,
    Minus,
    Multiply,
    Modulo,
    Exponentiation,
    Increment,
    Decrement,
    LeftShift,
    RightShift,
    UnsignedRightShift,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    Not,
    BitwiseNot,
    And,
    Or,
    NullCoalescing,
    Ternary,
    Colon,
    Assignment,
    PlusAssignment,
    MinusAssignment,
    MultiplyAssignment,
    ModuloAssignment,
    ExponentiationAssignment,
    LeftShiftAssignment,
    RightShiftAssignment,
    UnsignedRightShiftAssignment,
    BitwiseAndAssignment,
    BitwiseOrAssignment,
    BitwiseXorAssignment,
    AndAssignment,
    OrAssignment,
    XorAssignment,
    NullCoalescingAssignment,
    ArrowFunction,
    Division,
    DivisionAssignment,
    ClosedBrace,

    // https://tc39.es/ecma262/#sec-literals-numeric-literals
    Number,

    // https://tc39.es/ecma262/#sec-literals-string-literals
    String,
}

// FIXME: Support character/line numbers with tokens
internal struct Token
{
    public TokenType Type;
    public string Data;
}
