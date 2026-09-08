using Library.Domain.Interfaces;

namespace Library.Api.Endpoints;

public static class ExternalEndpoints
{
    public static void MapExternalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/external").WithTags("External");
        group.MapGet("/", async (IExternalStatusClient client) => 
        {
            try
            {
                var success = await client.CheckStatusAsync(CancellationToken.None);
                return Results.Ok(new { success });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Échec après toutes les tentatives : {ex.Message}");
            }
        });
    }
}