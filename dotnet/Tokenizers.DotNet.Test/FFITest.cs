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
                Assert.Equal(TokenizerErrorCode.Success, tokenizerResult.error_code);
                var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                sessionId = new string(str);
                Assert.True(sessionId.Length > 0);
            }
        }
    }
}
