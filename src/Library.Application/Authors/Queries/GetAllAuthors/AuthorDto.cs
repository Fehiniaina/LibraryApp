// Authors/Queries/GetAllAuthors/AuthorDto.cs
namespace Library.Application.Authors.Queries.GetAllAuthors;

public record AuthorDto(Guid Id, string FirstName, string LastName, int BookCount);