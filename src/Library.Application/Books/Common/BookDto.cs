namespace Library.Application.Books.Common;

public record BookDTO(
    Guid Id,
    string Title,
    string AuthorFullName,
    decimal Price
);