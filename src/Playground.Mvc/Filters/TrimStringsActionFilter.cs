using Microsoft.AspNetCore.Mvc.Filters;

namespace Playground.Mvc.Filters;

public class TrimStringsActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var key in context.ActionArguments.Keys.ToList())
        {
            if (context.ActionArguments[key] is string str)
            {
                context.ActionArguments[key] = str.Trim(); // MODIFIE l'argument réel
                Console.WriteLine($">>> [ActionFilter] Apres '{key}' nettoyé: '{str}' → '{str.Trim()}'");
            }
        }
    }
}
