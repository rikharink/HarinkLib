using System.Runtime.CompilerServices;

namespace HarinkLib.DataStructures;

public struct CircularBuffer<T> where T : struct
{
    private readonly T[] _buffer;
    private int _start;
    private int _end;

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

    public ReadOnlySpan<T> Read() => Read(Available, true);

    public ReadOnlySpan<T> Read(int length) => Read(length, true);

    public bool Read(ref Span<T> data)
    {
        if (data.Length > Available)
        {
            return false;
        }

        var end = WrapIndex(_start + data.Length);
        if (_start < end)
        {
            _buffer.AsSpan()[_start..end].CopyTo(data);
            Skip(data.Length);
        }
        else
        {
            _buffer.AsSpan()[_start..].CopyTo(data);
            _buffer.AsSpan()[..end].CopyTo(data[(_buffer.Length - _start)..]);
            Skip(data.Length);
        }

        return true;
    }

    public ReadOnlySpan<T> Peek() => Peek(Available);
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


    public int Free => _buffer.Length - Available;

    public void Skip(int count)
    {
        var lengthBefore = Available;
        _start = WrapIndex(_start + count);
        if (lengthBefore < Available)
        {
            _start = _end = 0;
        }
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int WrapIndex(int index)
    {
        return index % _buffer.Length;
    }
}