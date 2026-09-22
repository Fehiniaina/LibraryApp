using Library.Application.Authors.Commands.CreateAuthor;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Playground.Mvc.Filters;

public class BlockNegativePriceFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            Console.WriteLine($">>> [OnActionExecuted] Exception détectée: {context.Exception.Message}");
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("command", out var arg)
            && arg is CreateAuthorCommand command)
        {
            // Exemple arbitraire — bloque si FirstName contient "test"
            if (command.FirstName.Contains("BLOCK", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new BadRequestObjectResult("Prénom refusé par le Filter — l'Action ne s'exécutera JAMAIS.");
                // AUCUN "return" nécessaire — assigner "context.Result" SUFFIT à court-circuiter
            }
        }
    }
}