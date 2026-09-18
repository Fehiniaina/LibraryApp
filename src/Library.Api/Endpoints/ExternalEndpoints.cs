using Library.Domain.Interfaces;

namespace Library.Api.Endpoints;

internal static class ExternalEndpoints
{
    public static void MapExternalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/external").WithTags("External");
        group.MapGet("/", async (IExternalStatusClient client) => 
        {
            try
            {
                var success = await client.CheckStatusAsync(CancellationToken.None).ConfigureAwait(false);
                return Results.Ok(new { success });
            }
            catch (Exception)
            {
                throw;
            }
        });
    }
}