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
        var exceptions = new ConcurrentBag<Exception>();
        var totalCount = 0L;
        var timeToRun = TimeSpan.FromSeconds(3);
        var runTime = ExecuteInParallel(timeToRun);
        _output.WriteLine($"{totalCount / runTime.TotalSeconds} encodes/s");

        TimeSpan ExecuteInParallel(TimeSpan time)
        {
            // Use the golden ratio as a multiplier to slightly overprovision threads for better parallelism.
            // The choice of 1.618 is arbitrary and is based on a personal preference, any value between 1 and 2 will suffice.
            const double ThreadMultiplier = 1.618;
            var threads = new Thread[(int)Math.Ceiling(Environment.ProcessorCount * ThreadMultiplier)];
            var stopwatch = new Stopwatch();
            using (var cts = new CancellationTokenSource(time))
            {
                ThreadStart threadStart = () => Execute(cts.Token);
                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i] = new(threadStart);
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
            }

            if (!exceptions.IsEmpty)
            {
                throw new AggregateException(exceptions);
            }

            return stopwatch.Elapsed;
        }

        void Execute(CancellationToken cancellationToken)
        {
            try
            {
                var count = 0L;
                while (!cancellationToken.IsCancellationRequested)
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
