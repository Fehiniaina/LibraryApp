using Library.Application.Customers.Commands.CreateCustomer;
using MediatR;

namespace Library.Api.Endpoints
{
    public static class CustomerEndpoints
    {
        public static void MapCustomerEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/customers")
                .WithTags("Customers")
                .RequireAuthorization();

            group.MapPost("/", async (IMediator mediator, CreateCustomerCommand command) =>
            {
                var result = await mediator.Send(command);

                return result.Success
                    ? Results.Created($"/customers/{result.CustomerId}", result)
                    : Results.BadRequest(result.ErrorMessage);
            });
        }
    }
}
