using System.Collections.Concurrent;

namespace Tokenizers.DotNet.Test;

public sealed class TokenizerFixture
{
    private static readonly Func<ModelId, Lazy<Tokenizer>> TokenizerFactory = id => new(() => new(Models.GetFilePath(id)));

    private readonly ConcurrentDictionary<ModelId, Lazy<Tokenizer>> _tokenizers = new();

    public Tokenizer GetTokenizer(ModelId id) => _tokenizers.GetOrAdd(id, TokenizerFactory).Value;
}
