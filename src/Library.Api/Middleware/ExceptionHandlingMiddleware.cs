// src/Library.Api/Middleware/ExceptionHandlingMiddleware.cs
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée sur {Path}", context.Request.Path);

            var (statusCode, title) = ex switch
            {
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflit de concurrence."),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Requête invalide."),
                ArgumentException => (StatusCodes.Status400BadRequest, ex.Message), // souvent sûr d'exposer (validation métier)
                _ => (StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                title,
                status = statusCode,
                traceId = context.TraceIdentifier // permet de corréler avec tes vrais logs serveur
            });
        }
    }
}