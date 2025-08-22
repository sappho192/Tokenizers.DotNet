/*
 */

using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet.Test
{
    [Collection(TokenizerCollection.Name)]
    public class FFITest
    {
        private readonly TokenizerFixture _model;

        public FFITest(TokenizerFixture model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        [Fact]
        public unsafe void CheckInitialization()
        {
            var sessionId = string.Empty;
            var path = Models.GetFilePath(ModelId.KoGpt2);
            fixed (char* p = path)
            {
                var tokenizerResult = NativeMethods.tokenizer_initialize((ushort*)p, path.Length);
                try
                {
                    Assert.Equal(TokenizerErrorCode.Success, tokenizerResult.error_code);
                    sessionId = Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                }
                finally
                {
                    NativeMethods.free_u8_string(tokenizerResult.data);
                }

                Assert.True(sessionId.Length > 0);
            }
        }
    }
}
