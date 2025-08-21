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
        public void CheckInitialization()
        {
            var sessionId = string.Empty;
            unsafe
            {
                fixed (char* p = _model.FilePath)
                {
                    var tokenizerResult = NativeMethods.tokenizer_initialize((ushort*)p, _model.FilePath.Length);
                    Assert.Equal(TokenizerErrorCode.Success, tokenizerResult.error_code);
                    var str = Encoding.UTF8.GetString(tokenizerResult.data->AsSpan());
                    sessionId = new string(str);
                    Assert.True(sessionId.Length > 0);
                }
            }
        }
    }
}
