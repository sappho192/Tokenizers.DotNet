/*
NOTE: Currently the unit test for FFI is based on win-x64 only.
 */

using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet.Test
{
    public class FFITest
    {
        [Fact]
        public async Task CheckInitialization()
        {
            var hubName = "skt/kogpt2-base-v2";
            var filePath = "tokenizer.json";
            var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
            var sessionId = string.Empty;
            unsafe
            {
                fixed (char* p = fileFullPath)
                {
                    var tokenizerResult = NativeMethods.tokenizer_initialize((ushort*)p, fileFullPath.Length);
                    Assert.Equal(TokenizerErrorCode.Success, tokenizerResult.error_code);
                    var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                    sessionId = new string(str);
                    Assert.True(sessionId.Length > 0);
                }
            }
        }
    }
}
