namespace Tokenizers.DotNet.Test;

public class LifetimeTests
{
    [Fact]
    public void DisposeWorks()
    {
        var tokenizer = new Tokenizer(Models.GetFilePath(ModelId.KoGpt2));
        tokenizer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => tokenizer.Encode("abc"));
    }

    [Fact]
    public void MultiDisposeWorks()
    {
        var tokenizer = new Tokenizer(Models.GetFilePath(ModelId.KoGpt2));
        for (var i = 0; i < 7; i++)
        {
            tokenizer.Dispose();
        }

        Assert.Throws<ObjectDisposedException>(() => tokenizer.Encode("abc"));
    }
}
