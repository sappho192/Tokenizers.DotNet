using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public sealed class Tokenizer
    {
        private readonly string sessionId;

        /// <summary>
        /// Throws exception if error occured during tokenizer initialization.
        /// </summary>
        /// <param name="vocabPath"></param>
        public Tokenizer(string vocabPath)
        {
            unsafe
            {
                fixed (char* p = vocabPath)
                {
                    var tokenizerResult = NativeMethods.tokenizer_initialize((ushort*)p, vocabPath.Length);
                    if (tokenizerResult.error_code == 0)
                    {
                        try
                        {
                            sessionId = Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                        }
                        finally
                        {
                            NativeMethods.free_u8_string(tokenizerResult.data);
                        }
                    }
                    else
                    {
                        throw new TokenizerException(GetLastError(), (int)tokenizerResult.error_code);
                    }
                }
            }
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="TokenizerException"></exception>
        public uint[] Encode(string text)
        {
            uint[] result;
            unsafe
            {
                fixed (char* p = sessionId)
                {
                    fixed (char* pt = text)
                    {
                        var tokenizerResult = NativeMethods.tokenizer_encode((ushort*)p, sessionId.Length, (ushort*)pt, text.Length);
                        if (tokenizerResult.error_code == 0)
                        {
                            try
                            {
                                result = tokenizerResult.data->ToArray<uint>();
                            }
                            finally
                            {
                                NativeMethods.free_u8_string(tokenizerResult.data);
                            }
                        }
                        else
                        {
                            throw new TokenizerException(GetLastError(), (int)tokenizerResult.error_code);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public string Decode(uint[] tokens)
        {
            string result = string.Empty;
            unsafe
            {
                // Console.WriteLine($"Input tokens: {string.Join(", ", tokens)}");
                fixed (uint* p = tokens)
                {
                    fixed (char* cp = sessionId)
                    {
                        var tokenizerResult = NativeMethods.tokenizer_decode(
                            (ushort*)cp, sessionId.Length,
                            p, tokens.Length);
                        if (tokenizerResult.error_code == 0)
                        {
                            try
                            {
                                result = Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                            }
                            finally
                            {
                                NativeMethods.free_u8_string(tokenizerResult.data);
                            }
                        }
                        else
                        {
                            throw new TokenizerException(GetLastError(), (int)tokenizerResult.error_code);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            string result = string.Empty;
            unsafe
            {
                fixed (char* cp = sessionId)
                {
                    var tokenizerResult = NativeMethods.get_version((ushort*)cp, sessionId.Length);
                    if (tokenizerResult.error_code == 0)
                    {
                        try
                        {
                            result = Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                        }
                        finally
                        {
                            NativeMethods.free_u8_string(tokenizerResult.data);
                        }
                    }
                    else
                    {
                        throw new TokenizerException(GetLastError(), (int)tokenizerResult.error_code);
                    }
                }
            }

            return result;
        }

        private string GetLastError()
        {
            var result = string.Empty;
            unsafe
            {
                var errorBytes = NativeMethods.get_last_error_message();
                try
                {
                    result = Encoding.UTF8.GetString(errorBytes->ptr, errorBytes->length);
                }
                finally
                {
                    NativeMethods.free_u8_string(errorBytes);
                }
            }
            return result;
        }
    }
}
