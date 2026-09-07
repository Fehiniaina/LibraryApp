// src/Library.Application/Authors/Commands/CreateAuthorWithBook/CreateAuthorWithBookCommand.cs
using MediatR;

namespace Library.Application.Authors.Commands.CreateAuthorWithBook;

public record CreateAuthorWithBookCommand(string FirstName, string LastName, string BookTitle)
    : IRequest<CreateAuthorWithBookResult>;

public record CreateAuthorWithBookResult(bool Success, Guid? AuthorId, string? ErrorMessage);