
using System.Collections;

namespace Tokenizers.DotNet.Test;

internal sealed class TimeIterator : IEnumerable<TimeSpan>
{
    private readonly TimeProvider _timeProvider;

    public TimeIterator(TimeSpan limit, TimeProvider? timeProvider = null)
    {
        Limit = limit;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public TimeSpan Limit { get; }

    public IEnumerator<TimeSpan> GetEnumerator()
    {
        var start = _timeProvider.GetTimestamp();
        while (true)
        {
            var interval = _timeProvider.GetElapsedTime(start);
            if (interval > Limit)
            {
                break;
            }

            yield return interval;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
