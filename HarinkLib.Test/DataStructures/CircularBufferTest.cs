using HarinkLib.DataStructures;

namespace HarinkLib.Test.DataStructures;

public class CircularBufferTest
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenCapacityIsLessThanOne()
    {
        Assert.Throws<ArgumentException>(() => new CircularBuffer<int>(0));
    }

    [Fact]
    public void CapacityOne_ShouldWork()
    {
        var buffer = new CircularBuffer<int>(1);
        buffer.Write(new ReadOnlySpan<int>([1]));
        buffer.Write(new ReadOnlySpan<int>([2]));
        var readData = buffer.Read();
        Assert.Equal([2], readData);
    }

    [Fact]
    public void Read_MoreThanAvailable_ShouldThrowInvalidOperationException()
    {
        var buffer = new CircularBuffer<int>(10);
        Assert.Throws<InvalidOperationException>(() => buffer.Read(11));
    }
    
    [Fact]
    public void Read_IntoSpan_MoreThanAvailable_ShouldReturnFalse()
    {
        var buffer = new CircularBuffer<int>(10);
        var span = new Span<int>(new int[6]);
        var result = buffer.Read(ref span);
        Assert.False(result);
    }
    
    [Fact]
    public void Read_IntoSpan_ShouldWork()
    {
        var buffer = new CircularBuffer<int>(10);
        buffer.Write(new ReadOnlySpan<int>([1, 2, 3, 4, 5]));
        var span = new Span<int>(new int[5]);
        var result = buffer.Read(ref span);
        Assert.True(result);
        Assert.Equal([1, 2, 3, 4, 5], span);
    }

    [Fact]
    public void Write_ShouldReturnFalse_WhenDataBiggerThanBuffer()
    {
        var buffer = new CircularBuffer<int>(5);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5, 6]);
        var result = buffer.Write(data);
        Assert.False(result);
    }

    [Fact]
    public void Write_ShouldReturnTrue_WhenDataIsWritten()
    {
        var buffer = new CircularBuffer<int>(10);
        var result = buffer.Write(new ReadOnlySpan<int>([1, 2, 3, 4, 5]));
        Assert.True(result);
    }

    [Fact]
    public void Read_ShouldReturnWrittenData_WhenDataIsWrittenAndThenRead()
    {
        var buffer = new CircularBuffer<int>(10);
        var data = new ReadOnlySpan<int>([1, 2, 3, 4, 5]);
        buffer.Write(data);

        var readData = buffer.Read();
        Assert.Equal(data, readData);
    }

    [Fact]
    public void Read_ShouldReturnWhatsWritten_Sequentially()
    {
        var buffer = new CircularBuffer<int>(10);
        var data1 = new ReadOnlySpan<int>([1, 2, 3]);
        var data2 = new ReadOnlySpan<int>([4, 5, 6]);
        var data3 = new ReadOnlySpan<int>([7, 8, 9]);
        var data4 = new ReadOnlySpan<int>([10, 11, 12]);

        buffer.Write(data1);
        var readData1 = buffer.Read();
        Assert.Equal(data1, readData1);

        buffer.Write(data2);
        var readData2 = buffer.Read();
        Assert.Equal(data2, readData2);

        buffer.Write(data3);
        var readData3 = buffer.Read();
        Assert.Equal(data3, readData3);

        buffer.Write(data4);
        var readData4 = buffer.Read();
        Assert.Equal(data4, readData4);
    }
}