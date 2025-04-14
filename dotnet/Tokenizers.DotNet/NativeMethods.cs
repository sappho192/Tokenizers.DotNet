// <auto-generated>
// This code is generated by csbindgen.
// DON'T CHANGE THIS DIRECTLY.
// </auto-generated>
#pragma warning disable CS8500
#pragma warning disable CS8981
using System;
using System.Runtime.InteropServices;


namespace CsBindgen
{
    internal static unsafe partial class NativeMethods
    {
        const string __DllName = "hf_tokenizers";



        [DllImport(__DllName, EntryPoint = "alloc_u8_string", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern ByteBuffer* alloc_u8_string();

        [DllImport(__DllName, EntryPoint = "free_u8_string", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void free_u8_string(ByteBuffer* buffer);

        [DllImport(__DllName, EntryPoint = "csharp_to_rust_string", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void csharp_to_rust_string(ushort* utf16_str, int utf16_len);

        [DllImport(__DllName, EntryPoint = "csharp_to_rust_u32_array", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void csharp_to_rust_u32_array(uint* buffer, int len);

        [DllImport(__DllName, EntryPoint = "get_last_error_message", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern ByteBuffer* get_last_error_message();

        [DllImport(__DllName, EntryPoint = "tokenizer_initialize", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern TokenizerResult tokenizer_initialize(ushort* utf16_path, int utf16_path_len);

        [DllImport(__DllName, EntryPoint = "tokenizer_encode", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern TokenizerResult tokenizer_encode(ushort* _session_id, int _session_id_len, ushort* _text, int _text_len);

        [DllImport(__DllName, EntryPoint = "tokenizer_decode", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern TokenizerResult tokenizer_decode(ushort* _session_id, int _session_id_len, uint* _token_ids, int _token_ids_len);

        [DllImport(__DllName, EntryPoint = "get_version", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern TokenizerResult get_version(ushort* _session_id, int _session_id_len);

        [DllImport(__DllName, EntryPoint = "tokenizer_cleanup", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern TokenizerErrorCode tokenizer_cleanup(ushort* _session_id, int _session_id_len);


    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe partial struct ByteBuffer
    {
        public byte* ptr;
        public int length;
        public int capacity;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe partial struct TokenizerResult
    {
        public TokenizerErrorCode error_code;
        public ByteBuffer* data;
    }


    internal enum TokenizerErrorCode : uint
    {
        Success = 0,
        InvalidInput = 20001,
        InitializationError = 20002,
        InvalidSessionId = 20003,
        EncodingError = 20004,
        DecodingError = 20005,
    }


}
