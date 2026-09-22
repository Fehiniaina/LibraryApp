namespace Playground.Mvc.Filters;

using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Filters;

public class ResultTimingFilter : IResultFilter
{
    private Stopwatch? _stopwatch;

    public void OnResultExecuted(ResultExecutedContext context)
    {
        _stopwatch = Stopwatch.StartNew();
        Console.WriteLine($">>> [ResultFilter] AVANT écriture — Type de résultat: {context.Result.GetType().Name}");
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        _stopwatch?.Stop();
        Console.WriteLine($">>> [ResultFilter] APRÈS écriture — {_stopwatch?.ElapsedMilliseconds}ms pour sérialiser/rendre");
    }
}
