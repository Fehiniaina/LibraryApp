// src/Library.Application/Authors/Common/PagedResult.cs
namespace Library.Application.Shared;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);