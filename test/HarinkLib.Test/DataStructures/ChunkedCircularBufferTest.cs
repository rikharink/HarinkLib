using HarinkLib.DataStructures;

namespace HarinkLib.Test.DataStructures;

public class ChunkedCircularBufferTest
{
    [Fact]
    public void ReadChunk_ShouldReturnWrittenDataChunked_WhenDataIsWrittenAndThenRead()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        buffer.Write(data);

        Assert.Equal(2, buffer.ChunksAvailable);
        var readData = buffer.ReadChunk();
        Assert.Equal(1, buffer.ChunksAvailable);
        Assert.Equal(data[..5], readData);
        var readData2 = buffer.ReadChunk();
        Assert.Equal(data[5..], readData2);
        Assert.Equal(0, buffer.ChunksAvailable);
    }
    
    [Fact]
    public void ReadChunk_ShouldThrowInvalidOperationException_WhenNoChunksAvailable()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        Assert.Throws<InvalidOperationException>(() => buffer.ReadChunk());
    }
    
    [Fact]
    public void ReadChunk_IntoSpan_ShouldReturnFalse_WhenNoChunksAvailable()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var span = new Span<int>(new int[5]);
        var result = buffer.ReadChunk(ref span);
        Assert.False(result);
    }
    
    [Fact]
    public void ReadChunk_IntoSpan_ShouldWork()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        buffer.Write(data);
        
        var span = new Span<int>(new int[5]);
        var result = buffer.ReadChunk(ref span);
        Assert.True(result);
        Assert.Equal([1, 2, 3, 4, 5], span);
        
        var result2 = buffer.ReadChunk(ref span);
        Assert.True(result2);
        Assert.Equal([6, 7, 8, 9, 10], span);
    }

    [Fact]
    public void PeekChunk_ShouldReturnWrittenDataChunked_WhenDataIsWrittenAndThenPeeked()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        buffer.Write(data);

        Assert.Equal(2, buffer.ChunksAvailable);
        var readData = buffer.PeekChunk();
        Assert.Equal(2, buffer.ChunksAvailable);
        Assert.Equal(data[..5], readData);
    }

    [Fact]
    public void PeekChunk_ShouldThrowInvalidOperationException_WhenNoChunksAvailable()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        Assert.Throws<InvalidOperationException>(() => buffer.PeekChunk());
    }

    [Fact]
    public void PeekChunk_IntoSpan_ShouldReturnFalse_WhenNoChunksAvailable()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var span = new Span<int>(new int[5]);
        var result = buffer.PeekChunk(ref span);
        Assert.False(result);
    }

    [Fact]
    public void PeekChunk_IntoSpan_ShouldWork()
    {
        var buffer = new ChunkedCircularBuffer<int>(10, 5);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        buffer.Write(data);
        
        var span = new Span<int>(new int[5]);
        var result = buffer.PeekChunk(ref span);
        Assert.True(result);
        Assert.Equal([1, 2, 3, 4, 5], span);
    
        var result2 = buffer.PeekChunk(ref span);
        Assert.True(result2);
        Assert.Equal([1, 2, 3, 4, 5], span);
    }
}