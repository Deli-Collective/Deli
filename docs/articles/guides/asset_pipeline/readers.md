# Asset Readers
Asset readers (or just "readers") are delegates which deserialize for file formats. They are a part of the asset pipeline, and may also be
used by mods to directly access files.

> [!TIP]
> A completed asset reader can be found in [the example
> mod](https://github.com/Deli-Collective/Deli.ExampleMod/blob/master/Deli.ExampleMod/src/AssetReaders.cs).

## Creating a (Immediate) Reader
Readers come in many forms. Consider a simple static method for reading purely the data that is given:

```c#
using Deli.VFS;

public static byte ByteOf(IFileHandle file)
{
    using var raw = file.OpenRead();
    
    return raw.ReadByte();
} 
```

Some readers may need access to other readers to complete their duties (higher order functions). In this case, a closure class can be made:

```c#
using Deli;
using Deli.VFS;

public class ByteReaderClosure
{
    private readonly ImmediateReader<int> _intOf;
    
    public ByteReaderClosure(ImmediateReader<int> intOf)
    {
        _intOf = intOf;
    }
    
    public byte ByteOf(IFileHandle file)
    {
        return (byte) _intOf(file);
    }
}
```

## Creating a Delayed Reader
Delayed readers are more complex within the method body, but easy to setup. Just wrap the return value of the previous reader in a
@Deli.Runtime.Yielding.ResultYieldInstruction`1:

```c#
using Deli.Runtime.Yielding;
using Deli.VFS;

public static ResultYieldInstruction<byte[]> BytesOf(IFileHandle file)
{
    var raw = file.OpenRead();
    var buffer = new byte[stream.Length];

    return new AsyncYieldInstruction<Stream>(raw, (self, callback, state) => self.BeginRead(buffer, 0, buffer.Length, callback, state),
        (self, result) => self.EndRead(result)).CallbackWith(() =>
    {
        raw.Dispose();
        return buffer;
    });
}
```

> [!WARNING]
> Using statements mean that the object is disposed when the function returns. Do not use using statements for delayed readers, or the
> stream will close before the coroutine starts.