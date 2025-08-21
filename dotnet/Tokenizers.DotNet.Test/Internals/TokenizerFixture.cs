using System.Collections.Concurrent;

namespace Tokenizers.DotNet.Test;

public sealed class TokenizerFixture
{
    private readonly Func<ModelId, Lazy<Tokenizer>> _tokenizerFactory;
    private readonly ConcurrentDictionary<ModelId, Lazy<Tokenizer>> _tokenizers = new();

    public TokenizerFixture()
    {
        _tokenizerFactory = id => new(() => new(Models.GetFilePath(id)));
    }

    public Tokenizer GetTokenizer(ModelId id) => _tokenizers.GetOrAdd(id, _tokenizerFactory).Value;
}
