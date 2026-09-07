// Authors/Queries/GetAllAuthors/GetAllAuthorsQuery.cs
using MediatR;

namespace Library.Application.Authors.Queries.GetAllAuthors;

public record GetAllAuthorsQuery : IRequest<List<AuthorDto>>;