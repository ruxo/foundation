namespace RZ.Foundation.Types;

/// <summary>
/// Wrap an immutable type to make it refer-able.
/// </summary>
/// <typeparam name="T">A type which is supposed to be immutable, such as a value type, record, or any immutable types.</typeparam>
public sealed class MutableRef<T>
{
    public MutableRef() : this(default!) {}
    public MutableRef(T value) => Value = value;

    public T Value { get; set; }

    public static implicit operator T(MutableRef<T> value) => value.Value;
}

public static class MutableRef
{
    public static MutableRef<T> New<T>() => new();
    public static MutableRef<T> From<T>(T value) => new(value);
}