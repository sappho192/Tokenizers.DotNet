namespace Tokenizers.DotNet.Test;

public sealed class TokenizerFixture : IAsyncLifetime
{
    private const string FileName = "tokenizer.json";
    private const string ModelName = "skt/kogpt2-base-v2";

    private static readonly SemaphoreSlim Lock = new(1);
    private static readonly string ModelFileName = GetModelFileName();

    private readonly Lazy<Tokenizer> _tokenizer;
    private string? _filePath;

    public TokenizerFixture()
    {
        _tokenizer = new(() => new(FilePath));
    }

    public string FilePath => _filePath ?? throw new InvalidOperationException("Model path is not initialized.");

    public Tokenizer Tokenizer => _tokenizer.Value;

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        if (_filePath is not null)
        {
            return;
        }


        var tempPath = Path.GetTempPath() + "Tokenizers.DotNet.Test";
        var filePath = Path.Combine(tempPath, ModelFileName);
        if (!File.Exists(filePath))
        {
            await Download(tempPath, filePath);
        }

        _filePath = filePath;
    }

    private static async Task Download(string tempPath, string filePath)
    {
        Directory.CreateDirectory(tempPath);

        // Locking prevents multiple threads from downloading the file simultaneously, crashing tests, it may impede parallelism, but we really don't care for realistic scenarios
        await Lock.WaitAsync();
        try
        {
            // Download to temporary file and then atomically move to prevent broken files
            var tempFile = await HuggingFace.GetFileFromHub(ModelName, FileName, tempPath);
            File.Move(tempFile, filePath, true);
        }
        finally
        {
            try
            {
                Lock.Release();
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }

    private static string GetModelFileName()
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string([.. ModelName.Select(c => Array.IndexOf(invalidChars, c) < 0 ? c : '_')]) + "-" + FileName;
    }
}
