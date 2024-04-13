using OneOf;

namespace JSS.Lib.Execution;

/// <summary>
/// Used to represent either an abrupt completion or <see cref="T"/>. <br/>
/// This allows us to directly return any <see cref="T"/> instead of being wrapped in a value. <br/>
/// </summary>
/// <typeparam name="T">The type that the non-abrupt completion path should return</typeparam>
public sealed class AbruptOr<T> where T : notnull
{
    private AbruptOr(T value)
    {
        _value = value;
    }

    private AbruptOr(Completion abruptCompletion)
    {
        _value = abruptCompletion;
    }

    /// <summary>
    /// Used for implicitly creating an <see cref="AbruptOr{T}"/> from a non-abrupt completion value.
    /// </summary>
    public static implicit operator AbruptOr<T>(T value) => new(value);

    /// <summary>
    /// Used for implicitly creating an <see cref="AbruptOr{T}"/> from an abrupt completion.
    /// </summary>
    public static implicit operator AbruptOr<T>(Completion abruptCompletion) => new(abruptCompletion);

    /// <returns>
    /// <see cref="true"/> if <see cref="AbruptOr{T}"/> contains an abrupt completion, otherwise, <see cref="false"/>.
    /// </returns>
    public bool IsAbruptCompletion() { return _value.IsT1; }

    /// <summary>
    /// Returns the value that the non-abrupt completion path holds.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="AbruptOr{T}"/> is an abrupt completion.
    /// </exception>
    public T Value {
        get
        {
            return _value.AsT0;
        }
    }

    /// <summary>
    /// Returns the value that the abrupt completion path holds.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="AbruptOr{T}"/> is not an abrupt completion.
    /// </exception>
    public Completion Completion
    {
        get
        {
            return _value.AsT1;
        }
    }

    private readonly OneOf<T, Completion> _value;
}
