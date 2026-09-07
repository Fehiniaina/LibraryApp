namespace Library.Application.Authors.Common;

public record AuthorDto(Guid Id, string FirstName, string LastName, int BookCount);