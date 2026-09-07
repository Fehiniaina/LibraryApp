// Authors/Queries/GetAuthorsEager/GetAuthorsEagerQuery.cs
using Library.Application.Authors.Common;
using MediatR;

namespace Library.Application.Authors.Queries.SearchAuthors;

public record SearchAuthorsQuery(
    string? LastName,
    int? MinBookCount,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AuthorDto>>;