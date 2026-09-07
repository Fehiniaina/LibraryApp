// src/Library.Application/Authors/Commands/CreateAuthorWithBook/CreateAuthorWithBookCommand.cs
using MediatR;

namespace Library.Application.Authors.Commands.CreateAuthorWithBook;

public record CreateAuthorWithBookCommand(
    string FirstName,
    string LastName,
    string BookTitle,
    decimal Price,
    string Currency = CurrencyCodes.EUR
) : IRequest<CreateAuthorWithBookResult>;

public record CreateAuthorWithBookResult(bool Success, Guid? AuthorId, string? ErrorMessage);

public static class CurrencyCodes
{
    public const string EUR = "EUR";
    public const string USD = "USD";
    public const string GBP = "GBP";
}