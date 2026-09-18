namespace Library.Api.Middleware;

using FluentValidation;
using Library.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Middleware de gestion centralisée des exceptions HTTP.
/// Intercepte les exceptions connues et retourne une réponse JSON structurée.
/// </summary>
/// 
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Classe statique enregistrée via extension method — jamais instanciée directement.")]
internal sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">Le prochain middleware dans le pipeline.</param>
    /// <param name="logger">Le logger associé à ce middleware.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invoque le middleware et intercepte les exceptions pour retourner une réponse JSON structurée.
    /// </summary>
    /// <param name="context">Le contexte HTTP courant.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex).ConfigureAwait(false);
        }
        catch (CompanyNotFoundException ex)
        {
            await HandleKnownExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            await HandleKnownExceptionAsync(context, StatusCodes.Status409Conflict, "Conflit de concurrence.").ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            await HandleKnownExceptionAsync(context, StatusCodes.Status400BadRequest, "Requête invalide.").ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            await HandleKnownExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message).ConfigureAwait(false);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title = "Une ou plusieurs erreurs de validation sont survenues.",
            status = StatusCodes.Status400BadRequest,
            errors,
        }).ConfigureAwait(false);
    }

    private static async Task HandleKnownExceptionAsync(HttpContext context, int statusCode, string title)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title,
            status = statusCode,
            traceId = context.TraceIdentifier,
        }).ConfigureAwait(false);
    }
}