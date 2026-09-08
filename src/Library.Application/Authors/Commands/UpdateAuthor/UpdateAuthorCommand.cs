// src/Library.Application/Authors/Commands/UpdateAuthor/UpdateAuthorCommand.cs
using MediatR;

namespace Library.Application.Authors.Commands.UpdateAuthor;

public record UpdateAuthorCommand(Guid Id, string FirstName, string LastName) : IRequest<UpdateAuthorResult>;