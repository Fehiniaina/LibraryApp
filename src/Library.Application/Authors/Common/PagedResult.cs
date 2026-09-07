// src/Library.Application/Authors/Common/PagedResult.cs
namespace Library.Application.Authors.Common;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);