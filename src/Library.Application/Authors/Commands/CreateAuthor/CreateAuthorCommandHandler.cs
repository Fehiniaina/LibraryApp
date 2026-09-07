// Authors/Commands/CreateAuthor/CreateAuthorCommandHandler.cs
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using MediatR;

namespace Library.Application.Authors.Commands.CreateAuthor;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Guid>
{
    private readonly LibraryDbContext _db;

    public CreateAuthorCommandHandler(LibraryDbContext db) => _db = db;

    public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = new Author(request.FirstName, request.LastName);
        _db.Authors.Add(author);
        await _db.SaveChangesAsync(cancellationToken);
        return author.Id;
    }
}