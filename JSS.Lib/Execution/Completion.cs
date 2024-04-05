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
public sealed class Completion
{
    internal Completion(CompletionType type, Value value, string target)
    {
        Type = type;
        Value = value;
        Target = target;
    }

    override public bool Equals(object? obj)
    {
        if (obj is not Completion completion) { return false; }

        return Type == completion.Type && Value.Equals(completion.Value) && Target == completion.Target;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value, Target);
    }

    // 6.2.4.1 NormalCompletion ( value ), https://tc39.es/ecma262/#sec-normalcompletion
    static public Completion NormalCompletion(Value value)
    {
        return new Completion(CompletionType.Normal, value, "");
    }

    // NOTE: We use an implicit conversion operator as a syntaxic sugar for NormalCompletion in functions
    public static implicit operator Completion(Value value) => NormalCompletion(value);
    public static implicit operator Completion(bool value) => NormalCompletion(value);
    public static implicit operator Completion(double value) => NormalCompletion(value);
    public static implicit operator Completion(string value) => NormalCompletion(value);

    // 6.2.4.2 ThrowCompletion ( value ), https://tc39.es/ecma262/#sec-throwcompletion
    static public Completion ThrowCompletion(Value value)
    {
        return new Completion(CompletionType.Throw, value, "");
    }

    static public Completion BreakCompletion(Value value, string target)
    {
        return new Completion(CompletionType.Break, value, target);
    }

    static public Completion ContinueCompletion(Value value, string target)
    {
        return new Completion(CompletionType.Continue, value, target);
    }

    static public Completion ReturnCompletion(Value value)
    {
        return new Completion(CompletionType.Return, value, "");
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

    // 14.7.1.1 LoopContinues ( completion, FIXME: labelSet ), https://tc39.es/ecma262/#sec-loopcontinues
    public Boolean LoopContinues()
    {
        // 1. If completion.[[Type]] is NORMAL, return true.
        if (IsNormalCompletion())
        {
            return true;
        }

        // 2. If completion.[[Type]] is not CONTINUE, return false.
        if (!IsContinueCompletion())
        {
            return false;
        }

        // 3. If completion.[[Target]] is EMPTY, return true.
        if (Target.Length == 0)
        {
            return true;
        }

        // FIXME: 4. If labelSet contains completion.[[Target]], return true.

        // 5. Return false.
        return false;
    }

    public bool IsNormalCompletion() { return Type == CompletionType.Normal; }
    public bool IsBreakCompletion() { return Type == CompletionType.Break; }
    public bool IsContinueCompletion() { return Type == CompletionType.Continue; }
    public bool IsReturnCompletion() { return Type == CompletionType.Return; }
    public bool IsThrowCompletion() { return Type == CompletionType.Throw; }
    public bool IsAbruptCompletion() { return !IsNormalCompletion(); }

    public bool IsValueEmpty() {  return Value.IsEmpty(); }

    internal CompletionType Type { get; }
    public Value Value { get; private set; }
    public string Target { get; }
}

internal static class CompletionHelper
{
    // FIXME: Find out if there is some way in C# to do '?'

    // Implements the '!' spec steps, https://tc39.es/ecma262/#sec-returnifabrupt-shorthands
    static public Value MUST(Completion completion)
    {
        // 1. Let val be OperationName().

        // 2. Assert: val is a normal completion.
        Debug.Assert(completion.IsNormalCompletion());

        // 3. Set val to val.[[Value]].
        return completion.Value;
    }
}
