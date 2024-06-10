using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public class Tokenizer
    {
        public string Decode(uint[] tokens)
        {
            string result = string.Empty;
            unsafe
            {
                Console.WriteLine($"Input tokens: {string.Join(", ", tokens)}");
                fixed (uint* p = tokens)
                {
                    var decoded = NativeMethods.tokenizer_decode(p, tokens.Length);
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

            return result;
        }
    }
}
