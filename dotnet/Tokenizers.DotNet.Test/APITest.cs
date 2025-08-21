namespace Tokenizers.DotNet.Test
{
    public class APITest
    {
        [Fact(Skip = "Manual testing to prevent HF overload by CI")]
        public async Task DownloadTokenFile()
        {
            var hubName = "skt/kogpt2-base-v2";
            var filePath = "tokenizer.json";
            var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
            Assert.True(File.Exists(fileFullPath));
        }
    }
}