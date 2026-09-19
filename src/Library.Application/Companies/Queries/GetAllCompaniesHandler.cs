using Library.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Library.Application.Companies.Queries;

public class GetAllCompaniesHandler : IRequestHandler<GetAllCompaniesQuery, List<CompaniesDTO>>
{
    private readonly LibraryDbContext _context;

    public GetAllCompaniesHandler(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompaniesDTO>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Companies
            .AsNoTracking()
            .Select(c => new CompaniesDTO(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }
}

public record GetAllCompaniesQuery : IRequest<List<CompaniesDTO>>;

public record CompaniesDTO(Guid Id, string Name);