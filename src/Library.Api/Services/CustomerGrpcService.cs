// src/Library.Api/Services/CustomerGrpcService.cs
using Grpc.Core;

using Library.Api.Grpc;
using Library.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Library.Api.Services;

internal sealed class CustomerGrpcService : CustomerGrpc.CustomerGrpcBase
{
    private readonly LibraryDbContext _db;

    public CustomerGrpcService(LibraryDbContext db)
    {
        _db = db;
    }

    public override async Task<CustomerReply> GetCustomerDetails(CustomerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CustomerId, out var customerId))
        {
            return new CustomerReply { Found = false };
        }

        var customer = await _db.Customers
            .Include(c => c.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer is null)
        {
            return new CustomerReply { Found = false };
        }

        return new CustomerReply
        {
            Id = customer.Id.ToString(),
            Name = customer.Name,
            CompanyName = customer.Company.Name,
            Found = true,
        };
    }
}