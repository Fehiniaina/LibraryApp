// src/Library.Application/Authors/Queries/GetAuthorsLazy/GetAuthorsLazyQuery.cs
using Library.Application.Authors.Common;
using MediatR;

namespace Library.Application.Authors.Queries.GetAuthorsLazy;

public record GetAuthorsLazyQuery : IRequest<List<AuthorWithBooksDto>>;