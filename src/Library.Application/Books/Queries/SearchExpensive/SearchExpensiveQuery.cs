using Library.Application.Books.Common;
using Library.Application.Shared;
using MediatR;

namespace Library.Application.Books.Queries.SearchExpensive;

public record SearchExpensiveQuery(
    decimal Price,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BookDto>>;