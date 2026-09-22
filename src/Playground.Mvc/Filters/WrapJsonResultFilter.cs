using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Playground.Mvc.Filters;

public class WrapJsonResultFilter : IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            var originalValue = objectResult.Value;

            // ENVELOPPE le résultat original dans une structure enrichie
            objectResult.Value = new
            {
                data = originalValue,
                meta = new
                {
                    timestamp = DateTime.UtcNow,
                    requestId = Guid.NewGuid()
                }
            };

            Console.WriteLine(">>> [ResultFilter] Résultat ENVELOPPÉ avant sérialisation");
        }
    }
}
