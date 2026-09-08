// Authors/Queries/GetAllAuthors/GetAllAuthorsQuery.cs
using Library.Application.Authors.Common;
using MediatR;

namespace Library.Application.Authors.Queries.GetAllAuthors;

public record GetAllAuthorsQuery : IRequest<List<AuthorDto>>;