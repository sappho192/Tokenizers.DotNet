using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsBindgen
{
    // C# side span utility 
    // From https://github.com/Cysharp/csbindgen/blob/main/README.md
    partial struct ByteBuffer
    {
        public unsafe Span<byte> AsSpan()
        {
            return new Span<byte>(ptr, length);
        }

        public unsafe Span<T> AsSpan<T>()
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(ptr), length / Unsafe.SizeOf<T>());
        }
    }
}
