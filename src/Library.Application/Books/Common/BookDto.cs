using Library.Domain.ValueObjects;

namespace Library.Application.Books.Common;

public record BookDto(
    Guid Id,
    string Title,
    string AuthorFullName,
    decimal Price
);