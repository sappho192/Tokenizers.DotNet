using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public class Tokenizer
    {
        private Tokenizer() { }

        public Tokenizer(string vocabPath)
        {
            unsafe
            {
                fixed (char* p = vocabPath)
                {
                    NativeMethods.tokenizer_initialize((ushort*)p, vocabPath.Length);
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

        //public string GetVersion()
        //{
        //    string result = string.Empty;
        //    unsafe
        //    {
        //        var versionBytes = NativeMethods.get_version();
        //        try
        //        {
        //            var str = Encoding.UTF8.GetString(versionBytes->AsSpan());
        //            result = new string(str);
        //        }
        //        finally
        //        {
        //            NativeMethods.free_u8_string(versionBytes);
        //        }
        //    }

        //    return result;
        //}
    }
}
