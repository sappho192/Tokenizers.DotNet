namespace Tokenizers.DotNet.Test;

[CollectionDefinition(Name)]
public sealed class TokenizerCollection : ICollectionFixture<TokenizerFixture>
{
    public const string Name = "Default";
}
