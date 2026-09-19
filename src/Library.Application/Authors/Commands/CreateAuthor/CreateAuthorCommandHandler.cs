// Authors/Commands/CreateAuthor/CreateAuthorCommandHandler.cs
namespace Library.Application.Authors.Commands.CreateAuthor;

using Library.Domain.Entities;
using Library.Infrastructure.Persistence;

using MediatR;

// Primary Constructors CreateAuthorCommandHandler(LibraryDbContext db) >> (C# 12)
public class CreateAuthorCommandHandler(LibraryDbContext db) : IRequestHandler<CreateAuthorCommand, Guid>
{
    public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = new Author(request.FirstName, request.LastName);
        db.Authors.Add(author);
        await db.SaveChangesAsync(cancellationToken);
        return author.Id;
    }
}