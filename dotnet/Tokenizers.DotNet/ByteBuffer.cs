using System.Runtime.CompilerServices;

namespace CsBindgen
{
    partial struct ByteBuffer
    {
        public unsafe T[] ToArray<T>()
            where T : unmanaged
        {
            var array = new T[length / Unsafe.SizeOf<T>()];
            fixed (void* arrayPtr = array)
            {
                Unsafe.CopyBlock(arrayPtr, ptr, unchecked((uint)length));
            }

            return array;
        }
    }
}
