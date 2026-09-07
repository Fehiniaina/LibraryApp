using Library.Application.Authors.Common;
using MediatR;

namespace Library.Application.Authors.Queries.GetAuthorExplicit;

public record GetAuthorExplicitQuery(Guid AuthorId) : IRequest<AuthorWithBooksDto?>;