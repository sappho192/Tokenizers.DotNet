using Xunit.Abstractions;

namespace Tokenizers.DotNet.Test;

[Collection(TokenizerCollection.Name)]
public class ThreadSafetyTests
{
    private readonly TokenizerFixture _model;
    private readonly ITestOutputHelper _output;

    public ThreadSafetyTests(TokenizerFixture model, ITestOutputHelper output)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public void ParallelStress()
    {
        uint[] expected = [13612, 440, 441];
        const string Reference = "abc";
        var tokenizer = _model.GetTokenizer(ModelId.KoGpt2);
        var totalCount = 0L;
        var spentTime = TimeSpan.Zero;
        const double GoldenRatio = 1.618;
        Parallel.ForEach(
            new TimeIterator(TimeSpan.FromSeconds(3)),
            new() { MaxDegreeOfParallelism = (int)Math.Ceiling(Environment.ProcessorCount * GoldenRatio) },
            () => (Count: 0, Time: TimeSpan.Zero),
            (time, _, local) =>
            {
                Assert.Equal(expected, tokenizer.Encode(Reference));
                return (local.Count + 1, time > local.Time ? time : local.Time);
            },
            local =>
            {
                totalCount += local.Count;
                spentTime = spentTime > local.Time ? spentTime : local.Time;
            });
        _output.WriteLine($"{totalCount / spentTime.TotalSeconds} encodes/s");
    }
}
