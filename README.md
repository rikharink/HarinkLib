# HarinkLib

## Data Structures

### CircularBuffer

A circular buffer is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end. 
This structure lends itself to buffering data streams.

```csharp
var buffer = new CircularBuffer<int>(10);
buffer.Write(new ReadOnlySpan<int>([1, 2, 3, 4, 5]));
var result = buffer.Read();
// result is [1, 2, 3, 4, 5]
buffer.Write(new ReadOnlySpan<int>([6, 7, 8, 9, 10, 11]));
result = buffer.Read();
// result is [6, 7, 8, 9, 10, 11]
````

### ChunkedCircularBuffer

Same data structure under the hood as the CircularBuffer, but with a predefined chunk size.
You can only read back data in chunks of that size