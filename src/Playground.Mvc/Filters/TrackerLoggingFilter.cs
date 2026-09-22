using Microsoft.AspNetCore.Mvc.Filters;

using Playground.Mvc.Services;

namespace Playground.Mvc.Filters;

public class TrackerLoggingFilter : IActionFilter
{
    private readonly IRequestTracker _tracker;

    public TrackerLoggingFilter(IRequestTracker tracker)
    {
        _tracker = tracker;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine($">>> [Filter] RequestId: {_tracker.RequestId}");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
