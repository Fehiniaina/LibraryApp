using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Filters;

namespace Playground.Mvc.Filters;

public class TimingActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();

        Console.WriteLine($">>> [ActionFilter] AVANT — {context.ActionDescriptor.DisplayName}");

        var executedContext = await next();

        sw.Stop();
        Console.WriteLine($">>> [ActionFilter] APRÈS — {sw.ElapsedMilliseconds}ms d'exécution, Exception: {executedContext.Exception is not null}");
    }
}