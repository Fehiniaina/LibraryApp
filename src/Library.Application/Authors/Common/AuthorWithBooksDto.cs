// Authors/Common/AuthorWithBooksDto.cs
namespace Library.Application.Authors.Common;

public record AuthorWithBooksDto(Guid Id, string FirstName, string LastName, List<string> BookTitles);