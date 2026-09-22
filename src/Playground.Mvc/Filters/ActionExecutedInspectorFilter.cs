using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Playground.Mvc.Filters;

public class ActionExecutedInspectorFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            context.ExceptionHandled = true;
            context.Result = new ObjectResult(new { error = "Erreur interceptée par le Filter" }) { StatusCode = 500 };
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
