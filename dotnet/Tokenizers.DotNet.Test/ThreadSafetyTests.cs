using System.Collections.Concurrent;
using System.Diagnostics;

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

        var stopwatch = new Stopwatch();
        var exceptions = new ConcurrentBag<Exception>();
        var totalCount = 0L;
        ExecuteInParallel();
        _output.WriteLine($"{totalCount / stopwatch.Elapsed.TotalSeconds} encodes/s");

        void ExecuteInParallel()
        {
            // Use the golden ratio as a multiplier to slightly overprovision threads for better parallelism.
            const double ThreadMultiplier = 1.618;
            var threads = new Thread[(int)Math.Ceiling(Environment.ProcessorCount * ThreadMultiplier)];
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new(Execute);
            }

            stopwatch.Start();
            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();

            if (!exceptions.IsEmpty)
            {
                throw new AggregateException(exceptions);
            }
        }

        void Execute()
        {
            try
            {
                var count = 0L;
                while (stopwatch.Elapsed.TotalSeconds < 3)
                {
                    Assert.Equal(expected, tokenizer.Encode(Reference));
                    ++count;
                }

                Interlocked.Add(ref totalCount, count);
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        }
    }
}
