using JSS.Lib.AST.Values;
using System.Diagnostics;

namespace JSS.Lib.Execution;

internal enum CompletionType
{
    Normal,
    Break,
    Continue,
    Return,
    Throw
}

// 6.2.4 The Completion Record Specification Type, https://tc39.es/ecma262/#sec-completion-record-specification-type
internal sealed class Completion
{
    public Completion(CompletionType type, Value value, string target)
    {
        Type = type;
        Value = value;
        Target = target;
    }

    // 6.2.4.1 NormalCompletion ( value ), https://tc39.es/ecma262/#sec-normalcompletion
    static public Completion NormalCompletion(Value value)
    {
        return new Completion(CompletionType.Normal, value, "");
    }

    // 6.2.4.2 ThrowCompletion ( value ), https://tc39.es/ecma262/#sec-throwcompletion
    static public Completion ThrowCompletion(Value value)
    {
        return new Completion(CompletionType.Throw, value, "");
    }

    // 6.2.4.3 UpdateEmpty ( completionRecord, value ), https://tc39.es/ecma262/#sec-updateempty
    public void UpdateEmpty(Value value)
    {
        // 1. Assert: If completionRecord.[[Type]] is either RETURN or THROW, then completionRecord.[[Value]] is not EMPTY.
        Debug.Assert(!(IsReturnCompletion() || IsThrowCompletion()) || !IsValueEmpty());

        // 2. If completionRecord.[[Value]] is not EMPTY, return ? completionRecord.
        if (!IsValueEmpty()) return;

        // 3. Return Completion Record { [[Type]]: completionRecord.[[Type]], [[Value]]: value, [[Target]]: completionRecord.[[Target]] }.
        Value = value;
    }

    public bool IsNormalCompletion() { return Type == CompletionType.Normal; }
    public bool IsBreakCompletion() { return Type == CompletionType.Break; }
    public bool IsContinueCompletion() { return Type == CompletionType.Continue; }
    public bool IsReturnCompletion() { return Type == CompletionType.Return; }
    public bool IsThrowCompletion() { return Type == CompletionType.Throw; }
    public bool IsAbruptCompletion() { return !IsNormalCompletion(); }

    public bool IsValueEmpty() {  return Value.IsEmpty(); }

    public CompletionType Type { get; }
    public Value Value { get; private set; }
    public string Target { get; }
}
