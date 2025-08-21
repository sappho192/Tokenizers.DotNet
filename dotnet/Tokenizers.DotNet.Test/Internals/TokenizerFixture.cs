namespace Tokenizers.DotNet.Test;

public sealed class TokenizerFixture
{
    private const string ModelPath = "../../../../../test-assets/tokenizers";
    private const string ModelName = "skt/kogpt2-base-v2";
    private const string FileName = "tokenizer.json";

    private readonly Lazy<Tokenizer> _tokenizer;

    public TokenizerFixture()
    {
        FilePath = Path.GetFullPath(Path.Combine(ModelPath, ModelName, FileName));
        _tokenizer = new(() => new(FilePath));
    }

    public string FilePath { get; }

    public Tokenizer Tokenizer => _tokenizer.Value;
}
