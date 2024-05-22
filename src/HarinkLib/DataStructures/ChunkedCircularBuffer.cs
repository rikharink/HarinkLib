namespace HarinkLib.DataStructures;

/// <summary>
/// Represents a circular buffer data structure that stores data that can be read in chunks.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
public struct ChunkedCircularBuffer<T> where T : struct
{
    private CircularBuffer<T> _buffer;
    private readonly int _chunkSize;

    /// <summary>
    /// Gets a value indicating whether there are chunks available to read from the buffer.
    /// </summary>
    /// <value>True if there are chunks available to read, otherwise false.</value>
    public bool CanReadChunk => ChunksAvailable > 0;

    /// <summary>
    /// Gets the number of chunks available to read from the buffer.
    /// </summary>
    /// <value>The number of chunks available to read from the buffer.</value>
    public int ChunksAvailable => _buffer.Available / _chunkSize;


    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkedCircularBuffer{T}"/> struct with the specified capacity and chunk size.
    /// </summary>
    /// <param name="capacity">The maximum number of elements that the buffer can hold.</param>
    /// <param name="chunkSize">The size of the chunks that can be read from the buffer.</param>
    /// <exception cref="ArgumentException">Thrown when the capacity or chunk size is less than 1, or when the chunk size is greater than the capacity.</exception>
    public ChunkedCircularBuffer(int capacity, int chunkSize)
    {
        if (capacity <= 1)
        {
            throw new ArgumentException("Capacity must be greater than 1", nameof(capacity));
        }

        if (chunkSize <= 1)
        {
            throw new ArgumentException("Chunk size must be greater than 1", nameof(chunkSize));
        }

        if (chunkSize > capacity)
        {
            throw new ArgumentException("Chunk size must be less than or equal to capacity", nameof(chunkSize));
        }

        _buffer = new CircularBuffer<T>(capacity);
        _chunkSize = chunkSize;
    }

    /// <summary>
    /// See <see cref="CircularBuffer{T}.Write(ReadOnlySpan{T})"/>
    /// </summary>
    /// <param name="data"></param>
    public void Write(ReadOnlySpan<T> data)
    {
        _buffer.Write(data);
    }

    /// <summary>
    /// Reads a chunk of data from the buffer.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the chunk of data read from the buffer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no chunks available to read.</exception>
    public ReadOnlySpan<T> ReadChunk()
    {
        if(!CanReadChunk)
        {
            throw new InvalidOperationException("No chunks available to read");
        }
        
        var chunk = _buffer.Read(_chunkSize);
        return chunk;
    }

    /// <summary>
    /// Reads a chunk of data and writes it to the specified buffer.
    /// </summary>
    /// <param name="chunk">The buffer to write the chunk of data to.</param>
    /// <returns>True if the chunk was read successfully, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when the length of the referenced chunk is not equal to the chunk size of the circular buffer.</exception>
    public bool ReadChunk(ref Span<T> chunk)
    {
        if (chunk.Length != _chunkSize)
        {
            throw new ArgumentException("Chunk size must be equal to the chunk size of the buffer", nameof(chunk));
        }
        
        if(!CanReadChunk)
        {
            return false;
        }
        _buffer.Read(ref chunk);
        return true;
    }

    /// <summary>
    /// Peek at the next chunk of data in the buffer without consuming it.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> containing the next chunk of data in the buffer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no chunks available to peek at.</exception>
    public ReadOnlySpan<T> PeekChunk()
    {
        if(!CanReadChunk)
        {
            throw new InvalidOperationException("No chunks available to peek");
        }
        
        var chunk = _buffer.Peek(_chunkSize);
        return chunk;
    }

    /// <summary>
    /// Peek at the next chunk of data in the buffer without consuming it. Writes the chunk to the specified buffer.
    /// </summary>
    /// <param name="chunk">The buffer to write the chunk of data to.</param>
    /// <returns>True if the chunk was peeked successfully, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when the length of the referenced chunk is not equal to the chunk size of the circular buffer.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there are no chunks available to peek at.</exception>
    /// <remarks>
    /// If the chunk was peeked successfully, the chunk will be written to the specified buffer.
    /// </remarks>
    public bool PeekChunk(ref Span<T> chunk)
    {
        if (chunk.Length != _chunkSize)
        {
            throw new ArgumentException("Chunk size must be equal to the chunk size of the buffer", nameof(chunk));
        }
        
        if(!CanReadChunk)
        {
            return false;
        }

        _buffer.Peek(ref chunk);
        return true;
    }
}