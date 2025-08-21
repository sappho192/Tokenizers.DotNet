using System.Collections.Concurrent;

namespace Tokenizers.DotNet.Test;

internal static class Models
{
    private const string ModelPath = "../../../../../test-assets/tokenizers";
    private const string FileName = "tokenizer.json";

    private static readonly Func<ModelId, string> PathFactory =
        id =>
        {
            var modelName = id switch
            {
                ModelId.KoGpt2 => "skt/kogpt2-base-v2",
                ModelId.OaiGpt2 => "openai-community/gpt2",
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Invalid model ID.")
            };

            return Path.Combine(ModelPath, modelName, FileName);
        };

    private static readonly ConcurrentDictionary<ModelId, string> Paths = new();

    public static string GetFilePath(ModelId id) => Paths.GetOrAdd(id, PathFactory);
}
