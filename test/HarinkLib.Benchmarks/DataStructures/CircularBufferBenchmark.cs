namespace HarinkLib.Benchmarks.DataStructures;

[MemoryDiagnoser]
public class CircularBufferBenchmark
{
    private CircularBuffer<float> _buffer = new CircularBuffer<float>(44100 * 2);
    private float[] _data = new float[44100 * 2];
    
    public CircularBufferBenchmark()
    {
        var random = new Random();
        for(var i = 0; i < _data.Length; i++)
        {
            _data[i] = random.NextSingle() * 2.0f - 1.0f;
        }
        
        _buffer.Write(_data);
        _buffer.Write([-0.5f, 0.5f, -0.5f]);
    }

    [Benchmark]
    public void Write()
    {
        _buffer.Write(_data);
    }

    [Benchmark]
    public void Read()
    {
        _buffer.Peek();
    }
    
}