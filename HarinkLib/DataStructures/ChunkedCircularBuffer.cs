namespace HarinkLib.DataStructures;

public struct ChunkedCircularBuffer<T> where T : struct
{
    private CircularBuffer<T> _buffer;
    private readonly int _chunkSize;

    public bool CanReadChunk => ChunksAvailable > 0;
    public int ChunksAvailable => _buffer.Available / _chunkSize;

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

    public void Write(ReadOnlySpan<T> data)
    {
        _buffer.Write(data);
    }

    public ReadOnlySpan<T> ReadChunk()
    {
        if(!CanReadChunk)
        {
            throw new InvalidOperationException("No chunks available to read");
        }
        
        var chunk = _buffer.Read(_chunkSize);
        return chunk;
    }
    
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
}