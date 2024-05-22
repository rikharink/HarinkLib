using System.Runtime.CompilerServices;

namespace HarinkLib.DataStructures;

/// <summary>
/// Represents a circular buffer data structure.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
public struct CircularBuffer<T> where T : struct
{
    private readonly T[] _buffer;
    private int _start;
    private int _end;


    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> struct with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of elements that the buffer can hold.</param>
    /// <exception cref="ArgumentException">Thrown when the capacity is less than 1.</exception>
    public CircularBuffer(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        }

        _buffer = new T[capacity];
        _start = 0;
        _end = 0;
    }

    /// <summary>
    /// Writes the specified data to the buffer. 
    /// If the buffer is full, the data will be written to the beginning of the buffer (it wraps around).
    /// Input data should not be longer than the total buffer capacity.
    /// </summary>
    /// <param name="data">The data to be written to the buffer. This may not be longer than the total buffer capacity</param>
    /// <returns>True if the data was written successfully, otherwise false.</returns>
    public bool Write(ReadOnlySpan<T> data)
    {
        if (data.Length > _buffer.Length)
        {
            return false;
        }

        var remainingCapacity = _buffer.Length - _end;
        if (remainingCapacity < data.Length)
        {
            var overflow = data.Length - remainingCapacity;
            data[..remainingCapacity].CopyTo(_buffer.AsSpan()[_end..]);
            data[remainingCapacity..].CopyTo(_buffer);
            _end = overflow;
        }
        else
        {
            data.CopyTo(_buffer.AsSpan()[_end..]);
            _end += data.Length;
        }

        return true;
    }

    /// <summary>
    /// Reads and consumes the entire buffer and returns the data as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the data in the buffer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
    public ReadOnlySpan<T> Read() => Read(Available, true);

    /// <summary>
    /// Reads and consumes the specified number of elements from the buffer and returns the data as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="length">The number of elements to read</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the requested amount of data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the buffer does not contain enough data to read the requested length.</exception>
    public ReadOnlySpan<T> Read(int length) => Read(length, true);

    /// <summary>
    /// Reads and consumes the specified number of elements from the buffer and puts it in the provided reference <see cref="Span{T}"/>. 
    /// </summary>
    /// <param name="data">The <see cref="Span{T}"/> to write the data to.</param>
    /// <returns>True if the data was read successfully, otherwise false.</returns>
    public bool Read(ref Span<T> data) => Read(ref data, true);

    /// <summary>
    /// Peeks at the specified number of elements from the buffer and puts it in the provided reference <see cref="Span{T}"/>
    /// </summary>
    /// <param name="data">The <see cref="Span{T}"/> to write the data to.</param>
    /// <returns>True if the data was peeked successfully, otherwise false.</returns>
    public bool Peek(ref Span<T> data) => Read(ref data, false);

    private bool Read(ref Span<T> data, bool shouldSkip)
    {
        if (data.Length > Available)
        {
            return false;
        }

        var end = WrapIndex(_start + data.Length);
        if (_start < end)
        {
            _buffer.AsSpan()[_start..end].CopyTo(data);
            if (shouldSkip)
            {
                Skip(data.Length);
            }
        }
        else
        {
            _buffer.AsSpan()[_start..].CopyTo(data);
            _buffer.AsSpan()[..end].CopyTo(data[(_buffer.Length - _start)..]);
            if (shouldSkip)
            {
                Skip(data.Length);
            }
        }

        return true;
    }

    /// <summary>
    /// Peeks at the data in the buffer without consuming it.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the data in the buffer.</returns>
    public ReadOnlySpan<T> Peek() => Peek(Available);

    /// <summary>
    /// Peeks at the specified number of elements in the buffer without consuming it.
    /// </summary>
    /// <param name="length">The number of elements to peek at.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the requested amount of data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the buffer does not contain enough data to peek at the requested length.</exception>
    public ReadOnlySpan<T> Peek(int length) => Read(length, false);


    private ReadOnlySpan<T> Read(int length, bool forward)
    {
        if (length > Available)
        {
            throw new InvalidOperationException("Not enough data available to read requested length");
        }

        var end = WrapIndex(_start + length);
        if (_start < end)
        {
            var span = _buffer.AsSpan()[_start..end];
            if (forward)
            {
                Skip(length);
            }

            return span;
        }

        var temp = new T[length];
        _buffer[_start..].CopyTo(temp.AsSpan());
        _buffer[..end].CopyTo(temp.AsSpan(_buffer.Length - _start));
        if (forward)
        {
            Skip(length);
        }

        return temp.AsSpan();
    }


    /// <summary>
    /// Amount of free space in the buffer.
    /// </summary>
    /// <returns>The number of elements are not yet filled in the buffer.</returns>
    public int Free => _buffer.Length - Available;

    /// <summary>
    /// Skips the specified number of elements in the buffer.
    /// </summary>
    /// <param name="count">The number of elements to skip.</param>
    /// <remarks>
    /// This is mainly for using in combination with <see cref="Peek"/>.
    /// For when you only want to skip the peeked data if it was successfully processed for example.
    /// </remarks>
    public void Skip(int count)
    {
        var lengthBefore = Available;
        _start = WrapIndex(_start + count);
        if (lengthBefore < Available)
        {
            _start = _end = 0;
        }
    }

    /// <summary>
    /// The amount of available elements in the buffer.
    /// </summary>
    /// <returns>The number of elements that are currently in the buffer.</returns>
    public int Available
    {
        get
        {
            if (_start == _end)
            {
                return 0;
            }

            if (_start < _end)
            {
                return _end - _start;
            }

            return _buffer.Length - _start + _end;
        }
    }

    private int WrapIndex(int index)
    {
        return index % _buffer.Length;
    }
}