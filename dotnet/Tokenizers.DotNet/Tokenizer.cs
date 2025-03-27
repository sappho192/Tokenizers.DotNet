using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public class Tokenizer
    {
        private Tokenizer() { }
        private readonly string sessionId;

        public Tokenizer(string vocabPath)
        {
            unsafe
            {
                fixed (char* p = vocabPath)
                {
                    var session_id = NativeMethods.tokenizer_initialize((ushort*)p, vocabPath.Length);
                    try
                    {
                        var str = Encoding.UTF8.GetString(session_id->AsSpan());
                        sessionId = new string(str);
                    }
                    finally
                    {
                        NativeMethods.free_u8_string(session_id);
                    }
                }
            }
        }

        public uint[] Encode(string text)
        {
            unsafe
            {
                fixed (char* p = sessionId)
                {
                    fixed (char* pt = text)
                    {
                        var tokensRaw = NativeMethods.tokenizer_encode((ushort*)p, sessionId.Length, (ushort*)pt, text.Length);
                        var tokens = tokensRaw->AsSpan<uint>();
                        return tokens.ToArray();
                    }
                }
            }
        }

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
                        var decoded = NativeMethods.tokenizer_decode(
                            (ushort*)cp, sessionId.Length,
                            p, tokens.Length);
                        try
                        {
                            var str = Encoding.UTF8.GetString(decoded->AsSpan());
                            result = new string(str);
                        }
                        finally
                        {
                            NativeMethods.free_u8_string(decoded);
                        }
                    }
                }
            }

            return result;
        }

        public string GetVersion()
        {
            string result = string.Empty;
            unsafe
            {
                fixed(char* cp = sessionId)
                {
                    var versionBytes = NativeMethods.get_version((ushort*)cp, sessionId.Length);
                    try
                    {
                        var str = Encoding.UTF8.GetString(versionBytes->AsSpan());
                        result = new string(str);
                    }
                    finally
                    {
                        NativeMethods.free_u8_string(versionBytes);
                    }
                }
            }

            return result;
        }
    }
}
