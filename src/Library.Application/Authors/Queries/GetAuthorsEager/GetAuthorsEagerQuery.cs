// Authors/Queries/GetAuthorsEager/GetAuthorsEagerQuery.cs
using Library.Application.Authors.Common;
using MediatR;

namespace Library.Application.Authors.Queries.GetAuthorsEager;

public record GetAuthorsEagerQuery : IRequest<List<AuthorWithBooksDto>>;