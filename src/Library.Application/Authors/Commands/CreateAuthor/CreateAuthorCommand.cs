// Authors/Commands/CreateAuthor/CreateAuthorCommand.cs
using MediatR;

namespace Library.Application.Authors.Commands.CreateAuthor;

public record CreateAuthorCommand(string FirstName, string LastName) : IRequest<Guid>;