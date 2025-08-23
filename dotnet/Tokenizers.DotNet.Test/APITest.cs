namespace Tokenizers.DotNet.Test
{
    [Collection(TokenizerCollection.Name)]
    public class APITest
    {
        private readonly TokenizerFixture _model;

        public APITest(TokenizerFixture model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        [Fact(Skip = "Manual testing to prevent HF overload by CI")]
        public async Task DownloadTokenFile()
        {
            var hubName = "skt/kogpt2-base-v2";
            var filePath = "tokenizer.json";
            var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
            Assert.True(File.Exists(fileFullPath));
        }

        [Fact]
        public void MissingFileFailsCorrectly()
        {
            var exception = Assert.Throws<TokenizerException>(() => new Tokenizer("nonexistent.json"));
            Assert.NotEqual(0, exception.ErrorCode);
            Assert.StartsWith("Failed to load tokenizer: ", exception.Message);
        }

        [Fact]
        public void InvalidTextEncodingFailsCorrectly()
        {
            var tokenizer = _model.GetTokenizer(ModelId.KoGpt2);
            var exception = Assert.Throws<TokenizerException>(() => tokenizer.Encode("\uD801"));
            Assert.NotEqual(0, exception.ErrorCode);
            Assert.Equal("Invalid UTF-16 text string", exception.Message);
        }
    }
}