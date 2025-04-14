using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public class Tokenizer
    {
        private Tokenizer() { }
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
                        var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                        sessionId = new string(str);
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
            unsafe
            {
                fixed (char* p = sessionId)
                {
                    fixed (char* pt = text)
                    {
                        var tokenizerResult = NativeMethods.tokenizer_encode((ushort*)p, sessionId.Length, (ushort*)pt, text.Length);
                        if (tokenizerResult.error_code == 0)
                        {
                            var tokens = tokenizerResult.data->AsSpan<uint>();
                            return tokens.ToArray();
                        }
                        else
                        {
                            throw new TokenizerException(GetLastError(), (int)tokenizerResult.error_code);
                        }
                    }
                }
            }
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
                            var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                            result = new string(str);
                            NativeMethods.free_u8_string(tokenizerResult.data);
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
                        var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                        result = new string(str);
                        NativeMethods.free_u8_string(tokenizerResult.data);
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
                    var str = Encoding.UTF8.GetString(errorBytes->AsSpan());
                    result = new string(str);
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
